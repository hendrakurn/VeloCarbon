using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ScopeRunner _run;
    private bool _loading;
    public Shell Shell { get; }
    public DashboardViewModel Dashboard { get; }
    public InputViewModel Input { get; }
    public HistoryViewModel History { get; }
    public SimulationViewModel Simulation { get; }
    public FactorsViewModel Factors { get; }

    public ObservableCollection<UserProfile> Profiles { get; } = new();
    public ObservableCollection<DateTime> Months { get; } = new();
    public ObservableCollection<Vehicle> ProfileVehicles { get; } = new();

    [ObservableProperty] private string _newProfileName = "";
    [ObservableProperty] private string _editProfileName = "";
    [ObservableProperty] private string _newVehicleName = "";
    [ObservableProperty] private TransportMode _newVehicleType = TransportMode.Motorcycle;
    [ObservableProperty] private FuelType _newVehicleFuel = FuelType.Petrol;
    [ObservableProperty] private Vehicle? _selectedVehicle;
    [ObservableProperty] private string _editVehicleName = "";
    [ObservableProperty] private TransportMode _editVehicleType = TransportMode.Motorcycle;
    [ObservableProperty] private FuelType _editVehicleFuel = FuelType.Petrol;
    public TransportMode[] VehicleTypes { get; } = { TransportMode.Motorcycle, TransportMode.Car };
    public FuelType[] FuelTypes { get; } = Enum.GetValues<FuelType>();

    partial void OnSelectedVehicleChanged(Vehicle? value)
    {
        EditVehicleName = value?.Name ?? "";
        EditVehicleType = value?.VehicleType ?? TransportMode.Motorcycle;
        EditVehicleFuel = value?.FuelType ?? FuelType.Petrol;
    }

    public MainViewModel(ScopeRunner run, Shell shell)
    {
        _run = run; Shell = shell;
        Dashboard = new DashboardViewModel(run, shell);
        Input = new InputViewModel(run, shell, RefreshAllAsync);
        History = new HistoryViewModel(run, shell, RefreshAllAsync);
        Simulation = new SimulationViewModel(run, shell);
        Factors = new FactorsViewModel(run, shell);
        var now = MonthHelper.MonthStart(DateTime.Today);
        for (var i = 0; i < 24; i++) Months.Add(now.AddMonths(-i));
        Shell.PropertyChanged += async (_, e) =>
        {
            if (_loading) return;
            if (e.PropertyName is nameof(Shell.SelectedProfile) or nameof(Shell.SelectedMonth)) await RefreshAllAsync();
        };
    }

    public async Task InitializeAsync()
    {
        await LoadProfilesAsync();
        await Factors.LoadAsync();
    }

    private async Task LoadProfilesAsync(Guid? select = null)
    {
        await Shell.TryAsync(async () =>
        {
            var list = await _run.RunAsync(sp => sp.GetService<ProfileService>()!.GetAllAsync());
            _loading = true;
            Profiles.Clear();
            foreach (var p in list) Profiles.Add(p);
            Shell.SelectedProfile = Profiles.FirstOrDefault(p => p.Id == select) ?? Profiles.FirstOrDefault();
            _loading = false;
            await RefreshAllAsync();
        });
    }

    public async Task RefreshAllAsync()
    {
        if (Shell.ProfileId == null)
        {
            EditProfileName = "";
            ProfileVehicles.Clear();
            SelectedVehicle = null;
            Shell.Info("Belum ada profil. Buat profil di tab Profil & Kendaraan.");
            return;
        }
        EditProfileName = Shell.SelectedProfile?.Name ?? "";
        var prevVehicleId = SelectedVehicle?.Id;
        ProfileVehicles.Clear();
        foreach (var v in Shell.SelectedProfile?.Vehicles ?? new()) ProfileVehicles.Add(v);
        SelectedVehicle = ProfileVehicles.FirstOrDefault(v => v.Id == prevVehicleId) ?? ProfileVehicles.FirstOrDefault();
        await Shell.TryAsync(async () =>
        {
            await Input.LoadVehiclesAsync();
            await Dashboard.LoadAsync();
            await History.LoadAsync();
            await Simulation.LoadAsync();
        });
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        Guid? id = null;
        var ok = await Shell.TryAsync(async () =>
            id = (await _run.RunAsync(sp => sp.GetService<ProfileService>()!.CreateProfileAsync(NewProfileName))).Id,
            "Profil dibuat.");
        if (ok) { NewProfileName = ""; await LoadProfilesAsync(id); }
    }

    [RelayCommand]
    private async Task UpdateProfileAsync()
    {
        var id = Shell.ProfileId;
        if (id == null) { Shell.Error("Pilih profil terlebih dahulu."); return; }
        var ok = await Shell.TryAsync(async () =>
            await _run.RunAsync(sp => sp.GetService<ProfileService>()!.UpdateProfileAsync(id.Value, EditProfileName)),
            "Profil diperbarui.");
        if (ok) await LoadProfilesAsync(id);
    }

    [RelayCommand]
    private async Task AddVehicleAsync()
    {
        var ok = await Shell.TryAsync(async () =>
            await _run.RunAsync(sp => sp.GetService<ProfileService>()!.AddVehicleAsync(Shell.RequireProfile(), NewVehicleName, NewVehicleType, NewVehicleFuel)),
            "Kendaraan ditambahkan.");
        if (ok) { NewVehicleName = ""; await LoadProfilesAsync(Shell.ProfileId); }
    }

    [RelayCommand]
    private async Task UpdateVehicleAsync()
    {
        if (SelectedVehicle == null) { Shell.Error("Pilih kendaraan yang akan diubah."); return; }
        var vid = SelectedVehicle.Id;
        var ok = await Shell.TryAsync(async () =>
            await _run.RunAsync(sp => sp.GetService<ProfileService>()!.UpdateVehicleAsync(Shell.RequireProfile(), vid, EditVehicleName, EditVehicleType, EditVehicleFuel)),
            "Kendaraan diperbarui.");
        if (ok) { await LoadProfilesAsync(Shell.ProfileId); SelectedVehicle = ProfileVehicles.FirstOrDefault(v => v.Id == vid); }
    }

    [RelayCommand]
    private async Task DeleteVehicleAsync()
    {
        if (SelectedVehicle == null) { Shell.Error("Pilih kendaraan yang akan dihapus."); return; }
        if (!Shell.Confirm($"Hapus kendaraan '{SelectedVehicle.Name}'?")) return;
        var vid = SelectedVehicle.Id;
        var ok = await Shell.TryAsync(async () =>
            await _run.RunAsync(sp => sp.GetService<ProfileService>()!.DeleteVehicleAsync(Shell.RequireProfile(), vid)),
            "Kendaraan dihapus.");
        if (ok) await LoadProfilesAsync(Shell.ProfileId);
    }
}

internal static class ServiceProviderExt
{
    public static T? GetService<T>(this IServiceProvider sp) where T : class => (T?)sp.GetService(typeof(T));
}
