using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.ViewModels;

public record FactorRow(string Category, string Subcategory, string Value, string Unit, string Version, string Validity, string Source);

public partial class FactorsViewModel : ObservableObject
{
    private readonly ScopeRunner _run; private readonly Shell _shell;
    public ObservableCollection<FactorRow> Rows { get; } = new();
    public FactorsViewModel(ScopeRunner run, Shell shell) { _run = run; _shell = shell; }

    public async Task LoadAsync() => await _shell.TryAsync(async () =>
    {
        var list = await _run.RunAsync(sp => ((EmissionFactorService)sp.GetService(typeof(EmissionFactorService))!).GetAllAsync());
        Rows.Clear();
        foreach (var f in list)
            Rows.Add(new FactorRow(f.Category.ToString(), f.Subcategory, f.Value.ToString("0.###"), $"kgCO2e/{f.Unit}", f.Version,
                $"{f.ValidFrom:dd MMM yyyy} - {(f.ValidUntil?.ToString("dd MMM yyyy") ?? "sekarang")}", f.Source));
    });
}
