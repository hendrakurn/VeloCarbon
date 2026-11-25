using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.ViewModels;

public record SimTarget(Guid ActivityId, Guid? ApplianceId, ChangeType ChangeType, string Display);
public record SimChangeRow(SimTarget Target, decimal Percent) { public string Display => $"{Target.Display} → kurangi {Percent:0.##}%"; }
public record RecommendationRow(int Priority, string Title, string Action, string Reduction);
public record PastScenarioRow(string Name, string Month, string Baseline, string ScenarioValue, string Reduction);

public partial class SimulationViewModel : ObservableObject
{
    private readonly ScopeRunner _run; private readonly Shell _shell;
    public ObservableCollection<SimTarget> Targets { get; } = new();
    public ObservableCollection<SimChangeRow> Changes { get; } = new();
    public ObservableCollection<RecommendationRow> Recommendations { get; } = new();
    public ObservableCollection<PastScenarioRow> Past { get; } = new();

    [ObservableProperty] private SimTarget? _selectedTarget;
    [ObservableProperty] private SimChangeRow? _selectedChange;
    [ObservableProperty] private string _percent = "10";
    [ObservableProperty] private string _scenarioName = "Skenario hemat";
    [ObservableProperty] private bool _hasResult;
    [ObservableProperty] private string _baselineText = "-";
    [ObservableProperty] private string _scenarioText = "-";
    [ObservableProperty] private string _reductionText = "-";
    [ObservableProperty] private string _percentText = "-";

    public SimulationViewModel(ScopeRunner run, Shell shell) { _run = run; _shell = shell; }

    public async Task LoadAsync()
    {
        var id = _shell.RequireProfile();
        var (acts, past) = await _run.RunAsync(async sp => (
            await ((ActivityService)sp.GetService(typeof(ActivityService))!).GetMonthAsync(id, _shell.SelectedMonth),
            await ((ScenarioService)sp.GetService(typeof(ScenarioService))!).GetByProfileAsync(id)));
        Targets.Clear(); Changes.Clear(); Recommendations.Clear(); HasResult = false;
        foreach (var a in acts)
        {
            var label = $"{DashboardService.LabelOf(a)} ({a.ReferenceDate:dd MMM})";
            switch (a)
            {
                case MobilityActivity: Targets.Add(new SimTarget(a.Id, null, ChangeType.ReduceDistance, $"{label}: jarak/frekuensi")); break;
                case FuelPurchaseActivity: Targets.Add(new SimTarget(a.Id, null, ChangeType.ReduceLiters, $"{label}: liter BBM")); break;
                case ElectricityUsage { InputMethod: ElectricityInputMethod.MeterKWh }: Targets.Add(new SimTarget(a.Id, null, ChangeType.ReduceKWh, $"Listrik: total kWh")); break;
                case ElectricityUsage e:
                    foreach (var ap in e.Appliances) Targets.Add(new SimTarget(a.Id, ap.Id, ChangeType.ReduceApplianceHours, $"Listrik: jam pakai {ap.Name}")); break;
            }
        }
        SelectedTarget = Targets.FirstOrDefault();
        Past.Clear();
        foreach (var s in past.Where(s => s.Result != null))
            Past.Add(new PastScenarioRow(s.Name, s.BaselineStart.ToString("MMM yyyy"), Num.Kg(s.Result!.BaselineKgCO2e), Num.Kg(s.Result.ScenarioKgCO2e), Num.Kg(s.Result.ReductionKgCO2e)));
    }

    [RelayCommand]
    private void AddChange()
    {
        try
        {
            if (SelectedTarget == null) throw new DomainException("Pilih target perubahan. Catat aktivitas bulan ini terlebih dahulu.");
            var p = Num.Decimal(Percent, "Persentase");
            if (p <= 0 || p > 100) throw new DomainException("Persentase pengurangan harus di antara 0 (tidak termasuk) dan 100.");
            if (Changes.Any(c => c.Target == SelectedTarget)) throw new DomainException("Target ini sudah ada dalam skenario.");
            Changes.Add(new SimChangeRow(SelectedTarget, p));
            _shell.Info("Perubahan ditambahkan.");
        }
        catch (DomainException ex) { _shell.Error(ex.Message); }
    }

    [RelayCommand] private void RemoveChange() { if (SelectedChange != null) Changes.Remove(SelectedChange); }

    [RelayCommand]
    private async Task RunAsync()
    {
        var req = new ScenarioRequest(_shell.ProfileId ?? Guid.Empty, ScenarioName, _shell.SelectedMonth,
            Changes.Select(c => new ScenarioChangeInput(c.Target.ChangeType, c.Target.ActivityId, c.Target.ApplianceId, c.Percent)).ToList());
        Scenario? result = null;
        if (!await _shell.TryAsync(async () => result = await _run.RunAsync(sp => ((ScenarioService)sp.GetService(typeof(ScenarioService))!).RunAndSaveAsync(req)),
            "Simulasi selesai dan disimpan. Data aktivitas tidak diubah.")) return;
        var r = result!.Result!;
        BaselineText = Num.Kg(r.BaselineKgCO2e);
        ScenarioText = Num.Kg(r.ScenarioKgCO2e);
        ReductionText = Num.Kg(r.ReductionKgCO2e);
        PercentText = r.GetReductionPercentage() is { } pct ? $"{pct:0.##}%" : "tidak tersedia (baseline 0)";
        Recommendations.Clear();
        foreach (var rec in r.Recommendations.OrderBy(x => x.Priority))
            Recommendations.Add(new RecommendationRow(rec.Priority, rec.Title, rec.RecommendedAction, Num.Kg(rec.EstimatedReductionKgCO2e)));
        HasResult = true;
        await LoadPastAsync();
    }

    private async Task LoadPastAsync()
    {
        var id = _shell.RequireProfile();
        var past = await _run.RunAsync(sp => ((ScenarioService)sp.GetService(typeof(ScenarioService))!).GetByProfileAsync(id));
        Past.Clear();
        foreach (var s in past.Where(s => s.Result != null))
            Past.Add(new PastScenarioRow(s.Name, s.BaselineStart.ToString("MMM yyyy"), Num.Kg(s.Result!.BaselineKgCO2e), Num.Kg(s.Result.ScenarioKgCO2e), Num.Kg(s.Result.ReductionKgCO2e)));
    }
}
