using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.ViewModels;

public record ActivityRow(Guid Id, string Date, string Label, string Value, string Kg, string Source);

public partial class HistoryViewModel : ObservableObject
{
    private readonly ScopeRunner _run; private readonly Shell _shell; private readonly Func<Task> _refreshAll;
    public ObservableCollection<ActivityRow> Rows { get; } = new();
    [ObservableProperty] private ActivityRow? _selected;

    public HistoryViewModel(ScopeRunner run, Shell shell, Func<Task> refreshAll) { _run = run; _shell = shell; _refreshAll = refreshAll; }

    public async Task LoadAsync()
    {
        var id = _shell.RequireProfile();
        var list = await _run.RunAsync(sp => ((ActivityService)sp.GetService(typeof(ActivityService))!).GetMonthAsync(id, _shell.SelectedMonth));
        Rows.Clear();
        foreach (var a in list)
            Rows.Add(new ActivityRow(a.Id, a.ReferenceDate.ToString("dd MMM yyyy"), DashboardService.LabelOf(a),
                $"{a.GetActivityValue():0.##} {a.Unit}", a.Calculation == null ? "-" : Num.Kg(a.Calculation.ResultKgCO2e),
                a.Calculation?.EmissionFactor == null ? "-" : $"v{a.Calculation.EmissionFactor.Version} ({a.Calculation.EmissionFactor.Value:0.###}/{a.Unit})"));
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Selected == null) { _shell.Error("Pilih baris yang akan dihapus."); return; }
        if (!_shell.Confirm($"Hapus catatan '{Selected.Label}' ({Selected.Date})? Hasil emisinya ikut terhapus.")) return;
        var id = Selected.Id;
        if (await _shell.TryAsync(() => _run.RunAsync(sp => ((ActivityService)sp.GetService(typeof(ActivityService))!).DeleteAsync(id)), "Catatan dihapus."))
            await _refreshAll();
    }
}
