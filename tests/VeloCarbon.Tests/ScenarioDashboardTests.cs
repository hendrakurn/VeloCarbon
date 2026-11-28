using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.Tests;

public class ScenarioDashboardTests
{
    private readonly InMemoryStore _s = new();
    private readonly ActivityService _act;
    private readonly ScenarioService _scn;
    private readonly DashboardService _dash;
    private readonly Guid _p, _motor;
    private static readonly DateTime Sep = new(2026, 9, 1);

    public ScenarioDashboardTests()
    {
        _s.Factors.AddRange(TestData.SyntheticFactors());
        _act = new ActivityService(_s, _s, _s, new EmissionCalculatorService(), _s);
        _scn = new ScenarioService(_s, _s, _s, _s);
        _dash = new DashboardService(_s);
        var pr = new ProfileService(_s, _s);
        _p = pr.CreateProfileAsync("Uji").Result.Id;
        _motor = pr.AddVehicleAsync(_p, "Motor", TransportMode.Motorcycle, FuelType.Petrol).Result.Id;
    }

    [Theory] [InlineData(ChangeType.ReduceDistance)] [InlineData(ChangeType.ReduceLiters)] [InlineData(ChangeType.ReduceKWh)]
    public async Task Ten_percent_reduction_gives_90_percent_of_baseline(ChangeType type)
    {
        ActivityRecord a = type switch
        {
            ChangeType.ReduceDistance => await _act.AddDistanceAsync(new(_p, _motor, Sep, TransportMode.Motorcycle, 10, 2)),   // 4
            ChangeType.ReduceLiters => await _act.AddFuelPurchaseAsync(new(_p, _motor, Sep, FuelType.Petrol, 5, null)),        // 10
            _ => await _act.AddMeterElectricityAsync(new(_p, Sep, 20)),                                                        // 10
        };
        var sc = await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(type, a.Id, null, 10) }));
        var r = sc.Result!;
        Assert.Equal(a.Calculation!.ResultKgCO2e * 0.9m, r.ScenarioKgCO2e);
        Assert.Equal(10m, r.GetReductionPercentage());
    }

    [Fact] public async Task Appliance_hours_only_affect_the_selected_appliance()
    {
        var e = (ElectricityUsage)await _act.AddApplianceElectricityAsync(new(_p, Sep, new[]
        {
            new ApplianceInput("A", 100, 2, 5, 10), // 10 kWh = 5 kg
            new ApplianceInput("B", 50, 1, 4, 10),  //  2 kWh = 1 kg
        }));
        var a = e.Appliances.First(x => x.Name == "A");
        var sc = await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceApplianceHours, e.Id, a.Id, 50) }));
        Assert.Equal(6m, sc.Result!.BaselineKgCO2e);
        Assert.Equal(2.5m, sc.Result.ReductionKgCO2e);   // hanya alat A: 5 kg x 50%
        Assert.Equal(3.5m, sc.Result.ScenarioKgCO2e);
    }

    [Fact] public async Task Scenario_does_not_modify_source_data()
    {
        var a = await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceKWh, a.Id, null, 30) }));
        Assert.Equal(20m, ((ElectricityUsage)_s.Activities.Single()).UsageKWh);
        Assert.Equal(10m, _s.Activities.Single().Calculation!.ResultKgCO2e);
    }

    [Fact] public void Zero_baseline_has_no_percentage()
    {
        var r = new ScenarioResult { BaselineKgCO2e = 0, ReductionKgCO2e = 0 };
        Assert.Null(r.GetReductionPercentage());
    }

    [Fact] public async Task Incompatible_change_type_is_rejected()
    {
        var a = await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        await Assert.ThrowsAsync<DomainException>(() => _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceDistance, a.Id, null, 10) })));
    }

    [Theory] [InlineData(0)] [InlineData(-1)] [InlineData(101)]
    public async Task Reduction_percent_out_of_range_is_rejected(double pct)
    {
        var a = await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        await Assert.ThrowsAsync<DomainException>(() => _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceKWh, a.Id, null, (decimal)pct) })));
    }

    [Fact] public async Task Recommendations_are_ordered_by_impact()
    {
        var small = await _act.AddMeterElectricityAsync(new(_p, Sep, 2));                                           // 1 kg
        var big = await _act.AddFuelPurchaseAsync(new(_p, _motor, Sep, FuelType.Petrol, 50, null));                 // 100 kg
        var sc = await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[]
        {
            new ScenarioChangeInput(ChangeType.ReduceKWh, small.Id, null, 50),
            new ScenarioChangeInput(ChangeType.ReduceLiters, big.Id, null, 20),
        }));
        var recs = sc.Result!.Recommendations.OrderBy(r => r.Priority).ToList();
        Assert.Equal(20m, recs[0].EstimatedReductionKgCO2e);
        Assert.Equal(0.5m, recs[1].EstimatedReductionKgCO2e);
    }

    [Fact] public async Task Dashboard_total_and_shares()
    {
        await _act.AddDistanceAsync(new(_p, _motor, Sep, TransportMode.Motorcycle, 10, 2)); // 4
        await _act.AddMeterElectricityAsync(new(_p, Sep, 12));                              // 6
        var d = await _dash.GetSummaryAsync(_p, Sep);
        Assert.Equal(10m, d.TotalKgCO2e);
        Assert.Equal("Listrik", d.TopSource);
        Assert.Equal(60m, d.Categories.First(c => c.Label == "Listrik").SharePercent);
    }

    [Fact] public async Task Empty_month_is_zero_without_error()
    {
        var d = await _dash.GetSummaryAsync(_p, Sep);
        Assert.Equal(0m, d.TotalKgCO2e);
        Assert.Null(d.TopSource);
    }

    [Fact] public async Task Trend_returns_requested_months_in_order()
    {
        await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        var t = await _dash.GetTrendAsync(_p, Sep, 3);
        Assert.Equal(3, t.Count);
        Assert.Equal(Sep, t[2].Month);
        Assert.Equal(10m, t[2].TotalKgCO2e);
        Assert.Equal(0m, t[0].TotalKgCO2e);
    }

    [Fact] public async Task Factor_new_version_closes_previous_and_preserves_historical_records()
    {
        var factorSvc = new EmissionFactorService(_s, _s);
        var oldAct = await _act.AddMeterElectricityAsync(new(_p, Sep, 20)); // pakai t1 (0.5) -> 10 kg

        var v2 = await factorSvc.AddNewVersionAsync(new EmissionFactor
        {
            Category = EmissionCategory.Electricity, Subcategory = "Grid", Value = 0.75m, Unit = ActivityUnit.KWh,
            Source = "PLN 2026", Version = "t2", ValidFrom = new DateTime(2026, 10, 1)
        });
        var newAct = await _act.AddMeterElectricityAsync(new(_p, new DateTime(2026, 10, 1), 20)); // pakai t2 (0.75) -> 15 kg

        // Versi lama otomatis ditutup sehari sebelum versi baru berlaku
        var oldFactor = _s.Factors.First(f => f.Version == "t1" && f.Subcategory == "Grid");
        Assert.Equal(new DateTime(2026, 9, 30), oldFactor.ValidUntil);

        // Catatan lama tetap menampilkan versi & sumber semula
        Assert.Equal("t1", oldAct.Calculation!.EmissionFactor!.Version);
        Assert.Equal("sintetis-uji", oldAct.Calculation.EmissionFactor.Source);
        Assert.Equal(10m, oldAct.Calculation.ResultKgCO2e);

        // Catatan baru memakai versi & sumber baru
        Assert.Equal(v2.Id, newAct.Calculation!.EmissionFactorId);
        Assert.Equal("t2", newAct.Calculation.EmissionFactor!.Version);
        Assert.Equal("PLN 2026", newAct.Calculation.EmissionFactor.Source);
        Assert.Equal(15m, newAct.Calculation.ResultKgCO2e);
    }

    [Fact] public async Task Six_month_fixture_and_profile_switching_isolate_aggregates()
    {
        var pr = new ProfileService(_s, _s);
        var other = (await pr.CreateProfileAsync("Profil Kedua")).Id;

        // Profil utama: 6 bulan (Apr–Sep 2026), beberapa bulan kosong
        await _act.AddMeterElectricityAsync(new(_p, new DateTime(2026, 4, 1), 10));  // Apr: 5 kg
        await _act.AddMeterElectricityAsync(new(_p, new DateTime(2026, 7, 1), 40));  // Jul: 20 kg
        await _act.AddDistanceAsync(new(_p, _motor, Sep.AddDays(3), TransportMode.Motorcycle, 15, 2)); // Sep: 6 kg
        await _act.AddMeterElectricityAsync(new(_p, Sep, 28));                                         // Sep: 14 kg

        // Profil kedua di bulan yang sama tidak boleh mencampuri profil utama
        await _act.AddMeterElectricityAsync(new(other, Sep, 100)); // 50 kg

        var summary = await _dash.GetSummaryAsync(_p, Sep);
        Assert.Equal(20m, summary.TotalKgCO2e);
        Assert.Equal("Listrik", summary.TopSource);
        Assert.Equal(70m, summary.Categories.First(c => c.Label == "Listrik").SharePercent);
        Assert.Equal(30m, summary.Categories.First(c => c.Label == "Mobilitas - Motorcycle").SharePercent);

        var trend = await _dash.GetTrendAsync(_p, Sep, 6);
        Assert.Equal(6, trend.Count);
        Assert.Equal(new[] { 5m, 0m, 0m, 20m, 0m, 20m }, trend.Select(x => x.TotalKgCO2e).ToArray());

        var otherSummary = await _dash.GetSummaryAsync(other, Sep);
        Assert.Equal(50m, otherSummary.TotalKgCO2e);
        Assert.Equal("Listrik", otherSummary.TopSource);
    }

    [Fact] public async Task Missing_factor_message_mentions_unit_and_actionable_guidance()
    {
        _s.Factors.Clear();
        var ex = await Assert.ThrowsAsync<DomainException>(() => _act.AddMeterElectricityAsync(new(_p, Sep, 10)));
        Assert.Contains("KWh", ex.Message);
        Assert.Contains("2026-09-01", ex.Message);
    }

    [Fact] public async Task Cross_profile_target_is_rejected()
    {
        var other = (await new ProfileService(_s, _s).CreateProfileAsync("Profil Lain")).Id;
        var otherAct = await _act.AddMeterElectricityAsync(new(other, Sep, 20));
        await Assert.ThrowsAsync<DomainException>(() => _scn.RunAndSaveAsync(
            new(_p, "Lintas profil", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceKWh, otherAct.Id, null, 10) })));
    }

    [Fact] public async Task Empty_scenario_changes_is_rejected()
    {
        await Assert.ThrowsAsync<DomainException>(() => _scn.RunAndSaveAsync(
            new(_p, "Kosong", Sep, Array.Empty<ScenarioChangeInput>())));
    }

    [Fact] public async Task Zero_baseline_via_run_and_save_has_null_percentage()
    {
        var zeroMeter = await _act.AddMeterElectricityAsync(new(_p, Sep, 0));
        var sc = await _scn.RunAndSaveAsync(
            new(_p, "Baseline nol", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceKWh, zeroMeter.Id, null, 25) }));
        var r = sc.Result!;
        Assert.Equal(0m, r.BaselineKgCO2e);
        Assert.Equal(0m, r.ReductionKgCO2e);
        Assert.Equal(0m, r.ScenarioKgCO2e);
        Assert.Null(r.GetReductionPercentage());
    }

    [Fact] public async Task Two_change_scenario_persists_consistent_totals_and_reloads()
    {
        var ride = await _act.AddDistanceAsync(new(_p, _motor, Sep, TransportMode.Motorcycle, 10, 5)); // 10 kg
        var meter = await _act.AddMeterElectricityAsync(new(_p, Sep, 40));                            // 20 kg
        var saved = await _scn.RunAndSaveAsync(new(_p, "Hemat ganda", Sep, new[]
        {
            new ScenarioChangeInput(ChangeType.ReduceDistance, ride.Id, null, 20), // -2 kg
            new ScenarioChangeInput(ChangeType.ReduceKWh, meter.Id, null, 25),     // -5 kg
        }));

        var reloaded = (await _scn.GetByProfileAsync(_p)).Single();
        var r = reloaded.Result!;
        Assert.Equal(saved.Id, reloaded.Id);
        Assert.Equal(30m, r.BaselineKgCO2e);
        Assert.Equal(7m, r.ReductionKgCO2e);
        Assert.Equal(23m, r.ScenarioKgCO2e);
        Assert.Equal(r.BaselineKgCO2e - r.ReductionKgCO2e, r.ScenarioKgCO2e);
        Assert.Equal(7m / 30m * 100m, r.GetReductionPercentage());
        Assert.Equal(2, reloaded.Changes.Count);

        var recs = r.Recommendations.OrderBy(x => x.Priority).ToList();
        Assert.Equal(2, recs.Count);
        Assert.Equal(5m, recs[0].EstimatedReductionKgCO2e);
        Assert.Equal(2m, recs[1].EstimatedReductionKgCO2e);
    }
}
