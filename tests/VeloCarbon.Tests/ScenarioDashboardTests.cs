using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.Tests;

public class ScenarioDashboardTests
{
    private readonly InMemoryStore _s = new();
    private readonly ActivityService _act;
    private readonly ScenarioService _scn;
    private readonly DashboardService _dash;
    private readonly Guid _p, _motor;
    private static readonly DateTime Sep = new(2026, 9, 1);

    public ScenarioDashboardTests()
    {
        _s.Factors.AddRange(TestData.SyntheticFactors());
        _act = new ActivityService(_s, _s, _s, new EmissionCalculatorService(), _s);
        _scn = new ScenarioService(_s, _s, _s, _s);
        _dash = new DashboardService(_s);
        var pr = new ProfileService(_s, _s);
        _p = pr.CreateProfileAsync("Uji").Result.Id;
        _motor = pr.AddVehicleAsync(_p, "Motor", TransportMode.Motorcycle, FuelType.Petrol).Result.Id;
    }

    [Theory] [InlineData(ChangeType.ReduceDistance)] [InlineData(ChangeType.ReduceLiters)] [InlineData(ChangeType.ReduceKWh)]
    public async Task Ten_percent_reduction_gives_90_percent_of_baseline(ChangeType type)
    {
        ActivityRecord a = type switch
        {
            ChangeType.ReduceDistance => await _act.AddDistanceAsync(new(_p, _motor, Sep, TransportMode.Motorcycle, 10, 2)),   // 4
            ChangeType.ReduceLiters => await _act.AddFuelPurchaseAsync(new(_p, _motor, Sep, FuelType.Petrol, 5, null)),        // 10
            _ => await _act.AddMeterElectricityAsync(new(_p, Sep, 20)),                                                        // 10
        };
        var sc = await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(type, a.Id, null, 10) }));
        var r = sc.Result!;
        Assert.Equal(a.Calculation!.ResultKgCO2e * 0.9m, r.ScenarioKgCO2e);
        Assert.Equal(10m, r.GetReductionPercentage());
    }

    [Fact] public async Task Appliance_hours_only_affect_the_selected_appliance()
    {
        var e = (ElectricityUsage)await _act.AddApplianceElectricityAsync(new(_p, Sep, new[]
        {
            new ApplianceInput("A", 100, 2, 5, 10), // 10 kWh = 5 kg
            new ApplianceInput("B", 50, 1, 4, 10),  //  2 kWh = 1 kg
        }));
        var a = e.Appliances.First(x => x.Name == "A");
        var sc = await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceApplianceHours, e.Id, a.Id, 50) }));
        Assert.Equal(6m, sc.Result!.BaselineKgCO2e);
        Assert.Equal(2.5m, sc.Result.ReductionKgCO2e);   // hanya alat A: 5 kg x 50%
        Assert.Equal(3.5m, sc.Result.ScenarioKgCO2e);
    }

    [Fact] public async Task Scenario_does_not_modify_source_data()
    {
        var a = await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceKWh, a.Id, null, 30) }));
        Assert.Equal(20m, ((ElectricityUsage)_s.Activities.Single()).UsageKWh);
        Assert.Equal(10m, _s.Activities.Single().Calculation!.ResultKgCO2e);
    }

    [Fact] public void Zero_baseline_has_no_percentage()
    {
        var r = new ScenarioResult { BaselineKgCO2e = 0, ReductionKgCO2e = 0 };
        Assert.Null(r.GetReductionPercentage());
    }

    [Fact] public async Task Incompatible_change_type_is_rejected()
    {
        var a = await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        await Assert.ThrowsAsync<DomainException>(() => _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceDistance, a.Id, null, 10) })));
    }

    [Theory] [InlineData(0)] [InlineData(-1)] [InlineData(101)]
    public async Task Reduction_percent_out_of_range_is_rejected(double pct)
    {
        var a = await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        await Assert.ThrowsAsync<DomainException>(() => _scn.RunAndSaveAsync(new(_p, "s", Sep, new[] { new ScenarioChangeInput(ChangeType.ReduceKWh, a.Id, null, (decimal)pct) })));
    }

    [Fact] public async Task Recommendations_are_ordered_by_impact()
    {
        var small = await _act.AddMeterElectricityAsync(new(_p, Sep, 2));                                           // 1 kg
        var big = await _act.AddFuelPurchaseAsync(new(_p, _motor, Sep, FuelType.Petrol, 50, null));                 // 100 kg
        var sc = await _scn.RunAndSaveAsync(new(_p, "s", Sep, new[]
        {
            new ScenarioChangeInput(ChangeType.ReduceKWh, small.Id, null, 50),
            new ScenarioChangeInput(ChangeType.ReduceLiters, big.Id, null, 20),
        }));
        var recs = sc.Result!.Recommendations.OrderBy(r => r.Priority).ToList();
        Assert.Equal(20m, recs[0].EstimatedReductionKgCO2e);
        Assert.Equal(0.5m, recs[1].EstimatedReductionKgCO2e);
    }

    [Fact] public async Task Dashboard_total_and_shares()
    {
        await _act.AddDistanceAsync(new(_p, _motor, Sep, TransportMode.Motorcycle, 10, 2)); // 4
        await _act.AddMeterElectricityAsync(new(_p, Sep, 12));                              // 6
        var d = await _dash.GetSummaryAsync(_p, Sep);
        Assert.Equal(10m, d.TotalKgCO2e);
        Assert.Equal("Listrik", d.TopSource);
        Assert.Equal(60m, d.Categories.First(c => c.Label == "Listrik").SharePercent);
    }

    [Fact] public async Task Empty_month_is_zero_without_error()
    {
        var d = await _dash.GetSummaryAsync(_p, Sep);
        Assert.Equal(0m, d.TotalKgCO2e);
        Assert.Null(d.TopSource);
    }

    [Fact] public async Task Trend_returns_requested_months_in_order()
    {
        await _act.AddMeterElectricityAsync(new(_p, Sep, 20));
        var t = await _dash.GetTrendAsync(_p, Sep, 3);
        Assert.Equal(3, t.Count);
        Assert.Equal(Sep, t[2].Month);
        Assert.Equal(10m, t[2].TotalKgCO2e);
        Assert.Equal(0m, t[0].TotalKgCO2e);
    }
}
