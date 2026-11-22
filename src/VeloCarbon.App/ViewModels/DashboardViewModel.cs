using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.ViewModels;

public record CategoryRow(string Label, string KgText, string ShareText, double BarWidth);
public record TrendBar(string Month, string KgText, double BarHeight);

public partial class DashboardViewModel : ObservableObject
{
    private readonly ScopeRunner _run; private readonly Shell _shell;
    [ObservableProperty] private string _totalText = "0,00 kgCO2e";
    [ObservableProperty] private string _topSource = "-";
    [ObservableProperty] private bool _isEmpty = true;
    public ObservableCollection<CategoryRow> Categories { get; } = new();
    public ObservableCollection<TrendBar> Trend { get; } = new();

    public DashboardViewModel(ScopeRunner run, Shell shell) { _run = run; _shell = shell; }

    public async Task LoadAsync()
    {
        var id = _shell.RequireProfile();
        var (summary, trend) = await _run.RunAsync(async sp =>
        {
            var d = (DashboardService)sp.GetService(typeof(DashboardService))!;
            return (await d.GetSummaryAsync(id, _shell.SelectedMonth), await d.GetTrendAsync(id, _shell.SelectedMonth, 6));
        });
        TotalText = Num.Kg(summary.TotalKgCO2e);
        TopSource = summary.TopSource ?? "Belum ada data";
        IsEmpty = summary.Categories.Count == 0;
        Categories.Clear();
        foreach (var c in summary.Categories)
            Categories.Add(new CategoryRow(c.Label, Num.Kg(c.KgCO2e), $"{c.SharePercent:0.#}%", (double)c.SharePercent * 3.2));
        Trend.Clear();
        var max = trend.Max(t => t.TotalKgCO2e);
        foreach (var t in trend)
            Trend.Add(new TrendBar(t.Month.ToString("MMM yy"), Num.Kg(t.TotalKgCO2e), max == 0 ? 2 : Math.Max(2, (double)(t.TotalKgCO2e / max) * 140)));
    }
}
