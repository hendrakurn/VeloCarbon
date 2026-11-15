using VeloCarbon.Core.Enums;
using VeloCarbon.Core.Models;
using Xunit;

namespace VeloCarbon.Tests;

public class ActivityEmissionTests
{
    private static readonly EmissionFactor FuelFactor = new() { Value = 2.0m, Unit = ActivityUnit.Liter };
    private static readonly EmissionFactor GridFactor = new() { Value = 0.8m, Unit = ActivityUnit.KilowattHour };

    [Fact]
    public void Fuel_5Liter_x_2_0_equals_10kg()
    {
        var fuel = new FuelPurchaseActivity { LitersPurchased = 5m };
        Assert.Equal(10m, fuel.CalculateEmission(FuelFactor));
    }

    [Fact]
    public void ElectricityMeter_100kWh_x_0_8_equals_80kg()
    {
        var meter = new ElectricityUsage { InputMethod = ElectricityInputMethod.MeterKWh, UsageKWh = 100m };
        Assert.Equal(80m, meter.CalculateEmission(GridFactor));
    }

    [Fact]
    public void ElectricityAppliance_uses_appliance_estimate()
    {
        // 2 lampu x 100 W x 5 jam x 30 hari = 30 kWh -> 30 x 0.8 = 24 kgCO2e
        var estimate = new ElectricityUsage { InputMethod = ElectricityInputMethod.ApplianceEstimate };
        estimate.ApplianceUsages.Add(new ApplianceUsage { PowerWatt = 100m, Quantity = 2, HoursPerDay = 5m, DaysUsed = 30 });
        Assert.Equal(24m, estimate.CalculateEmission(GridFactor));
    }

    [Fact]
    public void Polymorphism_one_call_for_mixed_activities()
    {
        var activities = new List<(ActivityRecord Activity, EmissionFactor Factor)>
        {
            (new FuelPurchaseActivity { LitersPurchased = 5m }, FuelFactor),
            (new ElectricityUsage { UsageKWh = 100m }, GridFactor)
        };

        Assert.Equal(90m, activities.Sum(x => x.Activity.CalculateEmission(x.Factor)));
    }

    [Fact]
    public void Encapsulation_rejects_invalid_values()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FuelPurchaseActivity { LitersPurchased = 0m });
        Assert.Throws<ArgumentOutOfRangeException>(() => new ElectricityUsage { UsageKWh = -1m });
    }

    [Fact]
    public void Mismatched_factor_unit_is_rejected()
    {
        var fuel = new FuelPurchaseActivity { LitersPurchased = 5m };
        Assert.Throws<InvalidOperationException>(() => fuel.CalculateEmission(GridFactor));
    }
}
