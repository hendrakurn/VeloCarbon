using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.Tests;

public class ActivityServiceTests
{
    private readonly InMemoryStore _s = new();
    private readonly ActivityService _svc;
    private readonly ProfileService _profiles;
    private readonly Guid _profile;
    private readonly Guid _motor, _car;
    private static readonly DateTime Sep = new(2026, 9, 1);

    public ActivityServiceTests()
    {
        _s.Factors.AddRange(TestData.SyntheticFactors());
        _svc = new ActivityService(_s, _s, _s, new EmissionCalculatorService(), _s);
        _profiles = new ProfileService(_s, _s);
        _profile = _profiles.CreateProfileAsync("Uji").Result.Id;
        _motor = _profiles.AddVehicleAsync(_profile, "Motor", TransportMode.Motorcycle, FuelType.Petrol).Result.Id;
        _car = _profiles.AddVehicleAsync(_profile, "Mobil", TransportMode.Car, FuelType.Petrol).Result.Id;
    }

    [Fact] public async Task Distance_saves_activity_with_calculation()
    {
        var a = await _svc.AddDistanceAsync(new(_profile, _motor, Sep.AddDays(2), TransportMode.Motorcycle, 10, 2));
        Assert.Equal(4m, a.Calculation!.ResultKgCO2e);
        Assert.Single(_s.Activities);
    }

    [Fact] public async Task Fuel_after_distance_same_vehicle_month_is_rejected()
    {
        await _svc.AddDistanceAsync(new(_profile, _motor, Sep.AddDays(2), TransportMode.Motorcycle, 10, 2));
        var ex = await Assert.ThrowsAsync<DomainException>(() => _svc.AddFuelPurchaseAsync(new(_profile, _motor, Sep.AddDays(5), FuelType.Petrol, 5, null)));
        Assert.Contains("Distance", ex.Message);
        Assert.Single(_s.Activities);
    }

    [Fact] public async Task Distance_after_fuel_same_vehicle_month_is_rejected()
    {
        await _svc.AddFuelPurchaseAsync(new(_profile, _motor, Sep.AddDays(5), FuelType.Petrol, 5, null));
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddDistanceAsync(new(_profile, _motor, Sep.AddDays(6), TransportMode.Motorcycle, 3, 1)));
    }

    [Fact] public async Task Different_vehicle_or_month_can_use_other_method()
    {
        await _svc.AddDistanceAsync(new(_profile, _motor, Sep.AddDays(2), TransportMode.Motorcycle, 10, 2));
        await _svc.AddFuelPurchaseAsync(new(_profile, _car, Sep.AddDays(5), FuelType.Petrol, 5, 70000)); // kendaraan lain
        await _svc.AddFuelPurchaseAsync(new(_profile, _motor, Sep.AddMonths(1).AddDays(1), FuelType.Petrol, 4, null)); // bulan lain
        Assert.Equal(3, _s.Activities.Count);
    }

    [Fact] public async Task Deleting_last_activity_frees_the_method_choice()
    {
        var a = await _svc.AddDistanceAsync(new(_profile, _motor, Sep.AddDays(2), TransportMode.Motorcycle, 10, 2));
        await _svc.DeleteAsync(a.Id);
        Assert.Empty(_s.VehicleMonths);
        await _svc.AddFuelPurchaseAsync(new(_profile, _motor, Sep.AddDays(5), FuelType.Petrol, 5, null));
    }

    [Fact] public async Task Second_electricity_same_month_is_rejected_but_next_month_ok()
    {
        await _svc.AddMeterElectricityAsync(new(_profile, Sep, 20));
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddMeterElectricityAsync(new(_profile, Sep.AddDays(10), 30)));
        await _svc.AddMeterElectricityAsync(new(_profile, Sep.AddMonths(1), 25));
    }

    [Theory]
    [InlineData(0, 1)] [InlineData(-5, 1)] [InlineData(10, 0)]
    public async Task Distance_invalid_numbers_are_rejected(double km, int freq) =>
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddDistanceAsync(new(_profile, _motor, Sep, TransportMode.Motorcycle, (decimal)km, freq)));

    [Fact] public async Task Private_vehicle_mode_requires_vehicle() =>
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddDistanceAsync(new(_profile, null, Sep, TransportMode.Car, 5, 1)));

    [Fact] public async Task Bus_without_vehicle_is_allowed()
    {
        var a = await _svc.AddDistanceAsync(new(_profile, null, Sep, TransportMode.Bus, 10, 1));
        Assert.Equal(1m, a.Calculation!.ResultKgCO2e);
    }

    [Fact] public async Task Fuel_liters_must_be_positive() =>
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddFuelPurchaseAsync(new(_profile, _motor, Sep, FuelType.Petrol, 0, null)));

    [Theory] [InlineData(0, 5, 10)] [InlineData(100, 0, 10)] [InlineData(100, 25, 10)] [InlineData(100, 5, 0)] [InlineData(100, 5, 31)]
    public async Task Appliance_invalid_values_are_rejected(double watt, double hours, int days) =>
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddApplianceElectricityAsync(new(_profile, Sep,
            new[] { new ApplianceInput("X", (decimal)watt, 1, (decimal)hours, days) })));

    [Fact] public async Task Appliance_days_cannot_exceed_month_length() =>
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddApplianceElectricityAsync(new(_profile, new DateTime(2026, 2, 1),
            new[] { new ApplianceInput("X", 100, 1, 2, 29) })));

    [Fact] public async Task Missing_factor_gives_error_and_saves_nothing()
    {
        _s.Factors.Clear();
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddMeterElectricityAsync(new(_profile, Sep, 20)));
        Assert.Empty(_s.Activities);
    }

    [Fact] public async Task Vehicle_of_another_profile_is_rejected()
    {
        var other = (await _profiles.CreateProfileAsync("Lain")).Id;
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddDistanceAsync(new(other, _motor, Sep, TransportMode.Motorcycle, 5, 1)));
    }

    [Fact] public async Task Meter_zero_kwh_is_allowed_and_negative_is_rejected()
    {
        var zero = await _svc.AddMeterElectricityAsync(new(_profile, Sep, 0m));
        Assert.Equal(0m, zero.Calculation!.ResultKgCO2e);
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddMeterElectricityAsync(new(_profile, Sep.AddMonths(1), -1m)));
    }

    [Fact] public async Task Electricity_method_switch_works_after_delete()
    {
        var meter = await _svc.AddMeterElectricityAsync(new(_profile, Sep, 20m));
        await Assert.ThrowsAsync<DomainException>(() => _svc.AddApplianceElectricityAsync(new(_profile, Sep,
            new[] { new ApplianceInput("AC", 100, 1, 5, 10) })));
        await _svc.DeleteAsync(meter.Id);
        var app = await _svc.AddApplianceElectricityAsync(new(_profile, Sep, new[] { new ApplianceInput("AC", 100, 2, 5, 10) }));
        Assert.Equal(5m, app.Calculation!.ResultKgCO2e);
    }

    [Fact] public async Task Profile_can_be_updated_and_empty_name_rejected()
    {
        var updated = await _profiles.UpdateProfileAsync(_profile, "  Uji Baru  ");
        Assert.Equal("Uji Baru", updated.Name);
        await Assert.ThrowsAsync<DomainException>(() => _profiles.UpdateProfileAsync(_profile, "   "));
    }

    [Fact] public async Task Unused_vehicle_can_be_updated_and_deleted()
    {
        var v = await _profiles.AddVehicleAsync(_profile, "Motor Lama", TransportMode.Motorcycle, FuelType.Petrol);
        var updated = await _profiles.UpdateVehicleAsync(_profile, v.Id, "Mobil Solar", TransportMode.Car, FuelType.Diesel);
        Assert.Equal("Mobil Solar", updated.Name);
        Assert.Equal(TransportMode.Car, updated.VehicleType);
        Assert.Equal(FuelType.Diesel, updated.FuelType);

        await _profiles.DeleteVehicleAsync(_profile, v.Id);
        Assert.Null(await _s.GetVehicleAsync(v.Id));
    }

    [Fact] public async Task Vehicle_with_activities_cannot_be_deleted_or_change_type_fuel_but_can_rename()
    {
        var a = await _svc.AddDistanceAsync(new(_profile, _motor, Sep.AddDays(2), TransportMode.Motorcycle, 10, 2));

        // Hapus ditolak dengan pesan jelas
        var delEx = await Assert.ThrowsAsync<DomainException>(() => _profiles.DeleteVehicleAsync(_profile, _motor));
        Assert.Contains("aktivitas", delEx.Message, StringComparison.OrdinalIgnoreCase);

        // Ubah tipe atau BBM ditolak
        await Assert.ThrowsAsync<DomainException>(() => _profiles.UpdateVehicleAsync(_profile, _motor, "Motor", TransportMode.Car, FuelType.Petrol));
        await Assert.ThrowsAsync<DomainException>(() => _profiles.UpdateVehicleAsync(_profile, _motor, "Motor", TransportMode.Motorcycle, FuelType.Diesel));

        // Ubah nama tetap diperbolehkan
        var renamed = await _profiles.UpdateVehicleAsync(_profile, _motor, "Motor Kampus", TransportMode.Motorcycle, FuelType.Petrol);
        Assert.Equal("Motor Kampus", renamed.Name);

        // Setelah aktivitas dihapus, kendaraan bisa dihapus
        await _svc.DeleteAsync(a.Id);
        await _profiles.DeleteVehicleAsync(_profile, _motor);
        Assert.Null(await _s.GetVehicleAsync(_motor));
    }
}
