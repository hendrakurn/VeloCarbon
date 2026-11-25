using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public class ScenarioService
{
    private readonly IActivityRepository _activities;
    private readonly IProfileRepository _profiles;
    private readonly IScenarioRepository _scenarios;
    private readonly IUnitOfWork _uow;

    public ScenarioService(IActivityRepository a, IProfileRepository p, IScenarioRepository s, IUnitOfWork u)
    { _activities = a; _profiles = p; _scenarios = s; _uow = u; }

    /// <summary>Menjalankan dan menyimpan skenario. Data aktivitas sumber tidak pernah diubah.</summary>
    public async Task<Scenario> RunAndSaveAsync(ScenarioRequest req)
    {
        if (await _profiles.GetAsync(req.ProfileId) == null) throw new DomainException("Profil tidak ditemukan.");
        if (string.IsNullOrWhiteSpace(req.Name)) throw new DomainException("Nama skenario wajib diisi.");
        if (req.Changes.Count == 0) throw new DomainException("Skenario minimal memiliki satu perubahan.");
        var start = MonthHelper.MonthStart(req.Month);
        var activities = await _activities.GetByMonthAsync(req.ProfileId, start);

        var scenario = new Scenario
        {
            UserProfileId = req.ProfileId, Name = req.Name.Trim(), BaselineStart = start, BaselineEnd = MonthHelper.MonthEnd(start)
        };
        foreach (var c in req.Changes)
            scenario.Changes.Add(new ScenarioChange
            {
                ScenarioId = scenario.Id, ChangeType = c.ChangeType, TargetActivityRecordId = c.TargetActivityRecordId,
                TargetApplianceUsageId = c.TargetApplianceUsageId, ChangeValue = c.ReductionPercent
            });

        scenario.Result = RunScenario(scenario, activities);
        await _scenarios.AddAsync(scenario);
        await _uow.SaveChangesAsync();
        return scenario;
    }

    public Task<List<Scenario>> GetByProfileAsync(Guid profileId) => _scenarios.GetByProfileAsync(profileId);

    /// <summary>Fungsi murni: baseline = jumlah hasil emisi tersimpan; skenario = baseline dikurangi penurunan tiap aktivitas yang diubah.</summary>
    public static ScenarioResult RunScenario(Scenario scenario, IReadOnlyList<ActivityRecord> activities)
    {
        var baseline = activities.Where(a => a.Calculation != null).Sum(a => a.Calculation!.ResultKgCO2e);
        var result = new ScenarioResult { ScenarioId = scenario.Id, BaselineKgCO2e = baseline };
        var perChange = new List<(ScenarioChange Change, ActivityRecord Activity, decimal Reduction)>();

        foreach (var group in scenario.Changes.GroupBy(c => c.TargetActivityRecordId))
        {
            var activity = activities.FirstOrDefault(a => a.Id == group.Key)
                ?? throw new DomainException("Target perubahan tidak ditemukan pada bulan baseline.");
            var calc = activity.Calculation ?? throw new DomainException("Aktivitas target belum memiliki hasil emisi.");
            var factor = calc.EmissionFactor?.Value ?? (calc.ActivityValue == 0 ? 0 : calc.ResultKgCO2e / calc.ActivityValue);

            foreach (var c in group)
            {
                if (c.ChangeValue <= 0 || c.ChangeValue > 100) throw new DomainException("Persentase pengurangan harus di antara 0 (tidak termasuk) dan 100.");
                decimal before, after;
                switch (c.ChangeType)
                {
                    case ChangeType.ReduceDistance when activity is MobilityActivity:
                    case ChangeType.ReduceLiters when activity is FuelPurchaseActivity:
                        before = activity.GetActivityValue(); after = before * (1 - c.ChangeValue / 100m); break;
                    case ChangeType.ReduceKWh when activity is ElectricityUsage { InputMethod: ElectricityInputMethod.MeterKWh }:
                        before = activity.GetActivityValue(); after = before * (1 - c.ChangeValue / 100m); break;
                    case ChangeType.ReduceApplianceHours when activity is ElectricityUsage { InputMethod: ElectricityInputMethod.ApplianceEstimate } e:
                        var ap = e.Appliances.FirstOrDefault(x => x.Id == c.TargetApplianceUsageId)
                            ?? throw new DomainException("Alat target tidak ditemukan pada aktivitas listrik tersebut.");
                        before = ap.EstimateKWh(); after = before * (1 - c.ChangeValue / 100m); break;
                    default:
                        throw new DomainException($"Jenis perubahan {c.ChangeType} tidak cocok dengan aktivitas target ({activity.GetType().Name}).");
                }
                perChange.Add((c, activity, (before - after) * factor));
            }
        }

        result.ReductionKgCO2e = perChange.Sum(p => p.Reduction);
        result.ScenarioKgCO2e = baseline - result.ReductionKgCO2e;
        result.Recommendations = BuildRecommendations(result, perChange);
        return result;
    }

    private static List<Recommendation> BuildRecommendations(ScenarioResult result, List<(ScenarioChange Change, ActivityRecord Activity, decimal Reduction)> items)
    {
        var ordered = items.OrderByDescending(i => i.Reduction).ToList();
        var list = new List<Recommendation>();
        for (var n = 0; n < ordered.Count; n++)
        {
            var (c, a, r) = ordered[n];
            var action = c.ChangeType switch
            {
                ChangeType.ReduceDistance => $"Kurangi jarak/frekuensi perjalanan {((MobilityActivity)a).TransportMode} sebesar {c.ChangeValue:0.##}% atau beralih ke moda lebih rendah emisi.",
                ChangeType.ReduceLiters => $"Kurangi pembelian bahan bakar {((FuelPurchaseActivity)a).FuelType} sebesar {c.ChangeValue:0.##}% (gabungkan perjalanan, rawat kendaraan).",
                ChangeType.ReduceKWh => $"Hemat total pemakaian listrik sebesar {c.ChangeValue:0.##}% (matikan alat standby, gunakan alat hemat energi).",
                _ => $"Kurangi jam pakai alat terpilih sebesar {c.ChangeValue:0.##}%."
            };
            list.Add(new Recommendation
            {
                ScenarioResultId = result.Id, Priority = n + 1, TargetCategory = a.Category,
                Title = $"Prioritas {n + 1}: {DashboardService.LabelOf(a)}",
                Description = $"Estimasi pengurangan {r:0.00} kgCO2e pada bulan baseline.",
                RecommendedAction = action, EstimatedReductionKgCO2e = r
            });
        }
        return list;
    }
}
