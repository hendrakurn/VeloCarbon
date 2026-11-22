using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.ViewModels;

public partial class ApplianceRow : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _powerWatt = "";
    [ObservableProperty] private string _quantity = "1";
    [ObservableProperty] private string _hoursPerDay = "";
    [ObservableProperty] private string _daysUsed = "";
}

public partial class InputViewModel : ObservableObject
{
    private readonly ScopeRunner _run; private readonly Shell _shell; private readonly Func<Task> _refreshAll;
    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public TransportMode[] Modes { get; } = Enum.GetValues<TransportMode>();
    public FuelType[] FuelTypes { get; } = Enum.GetValues<FuelType>();

    // Perjalanan (jarak)
    [ObservableProperty] private TransportMode _mode = TransportMode.Motorcycle;
    [ObservableProperty] private Vehicle? _distanceVehicle;
    [ObservableProperty] private DateTime _distanceDate = DateTime.Today;
    [ObservableProperty] private string _distanceKm = "";
    [ObservableProperty] private string _frequency = "1";

    // Bahan bakar
    [ObservableProperty] private Vehicle? _fuelVehicle;
    [ObservableProperty] private DateTime _fuelDate = DateTime.Today;
    [ObservableProperty] private string _liters = "";
    [ObservableProperty] private string _totalCost = "";

    // Listrik: meter
    [ObservableProperty] private string _kwh = "";

    // Listrik: alat
    public ObservableCollection<ApplianceRow> Appliances { get; } = new();
    [ObservableProperty] private ApplianceRow? _selectedAppliance;

    public InputViewModel(ScopeRunner run, Shell shell, Func<Task> refreshAll)
    { _run = run; _shell = shell; _refreshAll = refreshAll; Appliances.Add(new ApplianceRow()); }

    public async Task LoadVehiclesAsync()
    {
        var id = _shell.RequireProfile();
        var p = await _run.RunAsync(sp => ((ProfileService)sp.GetService(typeof(ProfileService))!).GetAsync(id));
        Vehicles.Clear();
        foreach (var v in p?.Vehicles ?? new()) Vehicles.Add(v);
        DistanceVehicle = Vehicles.FirstOrDefault();
        FuelVehicle = Vehicles.FirstOrDefault();
    }

    private async Task SaveAsync(Func<ActivityService, Task> action, string ok, Action? clear = null)
    {
        if (await _shell.TryAsync(() => _run.RunAsync(sp => action((ActivityService)sp.GetService(typeof(ActivityService))!)), ok))
        { clear?.Invoke(); await _refreshAll(); _shell.Info(ok); }
    }

    [RelayCommand]
    private Task SaveDistanceAsync() => SaveAsync(s =>
    {
        var usesVehicle = Mode is TransportMode.Car or TransportMode.Motorcycle;
        return s.AddDistanceAsync(new DistanceInput(_shell.RequireProfile(), usesVehicle ? DistanceVehicle?.Id : null, DistanceDate, Mode,
            Num.Decimal(DistanceKm, "Jarak"), Num.Int(Frequency, "Frekuensi")));
    }, "Perjalanan tersimpan dan emisi dihitung.", () => { DistanceKm = ""; Frequency = "1"; });

    [RelayCommand]
    private Task SaveFuelAsync() => SaveAsync(s =>
    {
        var v = FuelVehicle ?? throw new DomainException("Pilih kendaraan.");
        decimal? cost = string.IsNullOrWhiteSpace(TotalCost) ? null : Num.Decimal(TotalCost, "Biaya");
        return s.AddFuelPurchaseAsync(new FuelPurchaseInput(_shell.RequireProfile(), v.Id, FuelDate, v.FuelType, Num.Decimal(Liters, "Liter"), cost));
    }, "Pembelian bahan bakar tersimpan dan emisi dihitung.", () => { Liters = ""; TotalCost = ""; });

    [RelayCommand]
    private Task SaveMeterAsync() => SaveAsync(s =>
        s.AddMeterElectricityAsync(new MeterElectricityInput(_shell.RequireProfile(), _shell.SelectedMonth, Num.Decimal(Kwh, "Total kWh"))),
        "Listrik (meter) tersimpan dan emisi dihitung.", () => Kwh = "");

    [RelayCommand] private void AddAppliance() => Appliances.Add(new ApplianceRow());
    [RelayCommand]
    private void RemoveAppliance()
    {
        if (SelectedAppliance != null) Appliances.Remove(SelectedAppliance);
        if (Appliances.Count == 0) Appliances.Add(new ApplianceRow());
    }

    [RelayCommand]
    private Task SaveAppliancesAsync() => SaveAsync(s =>
    {
        var list = Appliances.Select(a => new ApplianceInput(a.Name, Num.Decimal(a.PowerWatt, $"Daya '{a.Name}'"), Num.Int(a.Quantity, $"Jumlah '{a.Name}'"),
            Num.Decimal(a.HoursPerDay, $"Jam/hari '{a.Name}'"), Num.Int(a.DaysUsed, $"Hari pakai '{a.Name}'"))).ToList();
        return s.AddApplianceElectricityAsync(new ApplianceElectricityInput(_shell.RequireProfile(), _shell.SelectedMonth, list));
    }, "Listrik (estimasi alat) tersimpan dan emisi dihitung.", () => { Appliances.Clear(); Appliances.Add(new ApplianceRow()); });
}
