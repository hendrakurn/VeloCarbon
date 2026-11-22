using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.Tests;

public class CalculationTests
{
    private readonly EmissionCalculatorService _calc = new();
    private static readonly List<EmissionFactor> Factors = TestData.SyntheticFactors();
    private static EmissionFactor Factor(EmissionCategory c, string s) => Factors.First(f => f.Category == c && f.Subcategory == s);

    [Fact] public void Distance_10km_x2_x0_2_is_4()
    {
        var a = new MobilityActivity { TransportMode = TransportMode.Motorcycle, DistanceKm = 10, Frequency = 2, ActivityDate = new(2026, 9, 1) };
        Assert.Equal(4m, _calc.CalculateActivityEmission(a, Factor(EmissionCategory.Mobility, "Motorcycle")).ResultKgCO2e);
    }

    [Fact] public void Fuel_5L_x2_is_10()
    {
        var a = new FuelPurchaseActivity { FuelType = FuelType.Petrol, LitersPurchased = 5 };
        Assert.Equal(10m, _calc.CalculateActivityEmission(a, Factor(EmissionCategory.Mobility, "Petrol")).ResultKgCO2e);
    }

    [Fact] public void Meter_20kWh_x0_5_is_10()
    {
        var a = new ElectricityUsage { InputMethod = ElectricityInputMethod.MeterKWh, UsageKWh = 20 };
        Assert.Equal(10m, _calc.CalculateActivityEmission(a, Factor(EmissionCategory.Electricity, "Grid")).ResultKgCO2e);
    }

    [Fact] public void Appliance_100W_x2_x5h_x10d_is_10kWh_and_5kg()
    {
        var a = new ElectricityUsage { InputMethod = ElectricityInputMethod.ApplianceEstimate };
        a.Appliances.Add(new ApplianceUsage { Name = "AC", PowerWatt = 100, Quantity = 2, HoursPerDay = 5, DaysUsed = 10 });
        a.RecalculateFromAppliances();
        Assert.Equal(10m, a.UsageKWh);
        Assert.Equal(5m, _calc.CalculateActivityEmission(a, Factor(EmissionCategory.Electricity, "Grid")).ResultKgCO2e);
    }

    [Fact] public void Two_appliances_are_summed_before_factor()
    {
        var a = new ElectricityUsage { InputMethod = ElectricityInputMethod.ApplianceEstimate };
        a.Appliances.Add(new ApplianceUsage { Name = "A", PowerWatt = 100, Quantity = 2, HoursPerDay = 5, DaysUsed = 10 });
        a.Appliances.Add(new ApplianceUsage { Name = "B", PowerWatt = 50, Quantity = 1, HoursPerDay = 4, DaysUsed = 10 });
        a.RecalculateFromAppliances();
        Assert.Equal(12m, a.UsageKWh);
        Assert.Equal(6m, _calc.CalculateActivityEmission(a, Factor(EmissionCategory.Electricity, "Grid")).ResultKgCO2e);
    }

    [Fact] public void Unit_mismatch_is_rejected()
    {
        var a = new FuelPurchaseActivity { FuelType = FuelType.Petrol, LitersPurchased = 5 };
        var wrong = TestData.F(EmissionCategory.Mobility, "Petrol", 2m, ActivityUnit.Km);
        Assert.Throws<DomainException>(() => _calc.CalculateActivityEmission(a, wrong));
    }

    [Fact] public void Negative_factor_is_rejected()
    {
        var a = new ElectricityUsage { UsageKWh = 1 };
        Assert.Throws<DomainException>(() => _calc.CalculateActivityEmission(a, TestData.F(EmissionCategory.Electricity, "Grid", -1m, ActivityUnit.KWh)));
    }

    [Fact] public void Resolve_missing_factor_throws_not_zero()
    {
        var a = new MobilityActivity { TransportMode = TransportMode.Train, DistanceKm = 1, ActivityDate = new(2026, 9, 1) };
        Assert.Throws<DomainException>(() => EmissionFactorService.Resolve(Factors, a));
    }

    [Fact] public void Resolve_ambiguous_factor_throws()
    {
        var two = new List<EmissionFactor> { TestData.F(EmissionCategory.Electricity, "Grid", 0.5m, ActivityUnit.KWh), TestData.F(EmissionCategory.Electricity, "Grid", 0.6m, ActivityUnit.KWh) };
        var a = new ElectricityUsage { UsageKWh = 1, PeriodStart = new(2026, 9, 1) };
        Assert.Throws<DomainException>(() => EmissionFactorService.Resolve(two, a));
    }

    [Fact] public void Resolve_respects_validity_dates()
    {
        var old = TestData.F(EmissionCategory.Electricity, "Grid", 0.5m, ActivityUnit.KWh); old.ValidUntil = new DateTime(2026, 8, 31);
        var cur = TestData.F(EmissionCategory.Electricity, "Grid", 0.4m, ActivityUnit.KWh); cur.ValidFrom = new DateTime(2026, 9, 1);
        var sep = new ElectricityUsage { UsageKWh = 1, PeriodStart = new(2026, 9, 1) };
        var aug = new ElectricityUsage { UsageKWh = 1, PeriodStart = new(2026, 8, 1) };
        Assert.Equal(0.4m, EmissionFactorService.Resolve(new[] { old, cur }, sep).Value);
        Assert.Equal(0.5m, EmissionFactorService.Resolve(new[] { old, cur }, aug).Value);
    }
}
