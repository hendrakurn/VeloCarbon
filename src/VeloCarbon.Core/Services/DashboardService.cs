using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public class DashboardService
{
    private readonly IActivityRepository _activities;
    public DashboardService(IActivityRepository activities) => _activities = activities;

    public async Task<DashboardSummary> GetSummaryAsync(Guid profileId, DateTime month)
    {
        var start = MonthHelper.MonthStart(month);
        var list = await _activities.GetByMonthAsync(profileId, start);
        return Summarize(start, list);
    }

    public static DashboardSummary Summarize(DateTime monthStart, IEnumerable<ActivityRecord> activities)
    {
        var rows = activities.Where(a => a.Calculation != null)
            .GroupBy(a => (a.Category, Label: LabelOf(a)))
            .Select(g => (g.Key.Category, g.Key.Label, Kg: g.Sum(a => a.Calculation!.ResultKgCO2e)))
            .OrderByDescending(r => r.Kg).ToList();
        var total = rows.Sum(r => r.Kg);
        var cats = rows.Select(r => new CategoryShare(r.Label, r.Category, r.Kg, total == 0 ? 0 : r.Kg / total * 100m)).ToList();
        return new DashboardSummary(monthStart, total, cats, cats.Count > 0 && cats[0].KgCO2e > 0 ? cats[0].Label : null);
    }

    public static string LabelOf(ActivityRecord a) => a switch
    {
        MobilityActivity m => $"Mobilitas - {m.TransportMode}",
        FuelPurchaseActivity f => $"Mobilitas - BBM {f.FuelType}",
        _ => "Listrik"
    };

    public async Task<List<TrendPoint>> GetTrendAsync(Guid profileId, DateTime endMonth, int months)
    {
        if (months < 1 || months > 36) throw new DomainException("Jumlah bulan tren harus 1-36.");
        var end = MonthHelper.MonthStart(endMonth);
        var result = new List<TrendPoint>();
        for (var i = months - 1; i >= 0; i--)
        {
            var m = end.AddMonths(-i);
            var list = await _activities.GetByMonthAsync(profileId, m);
            result.Add(new TrendPoint(m, list.Where(a => a.Calculation != null).Sum(a => a.Calculation!.ResultKgCO2e)));
        }
        return result;
    }
}
