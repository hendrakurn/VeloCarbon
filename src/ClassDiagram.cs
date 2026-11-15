using System;
using System.Collections.Generic;
using System.Linq;
namespace VeloCarbon.Core;

public enum ActivityUnit
{
    Liter, KilowattHour
}
public enum ChangeType
{
    ReduceFuelPercent, ReduceElectricityPercent, ReduceApplianceHoursPercent
}
public enum ElectricityInputMethod
{
    MeterKWh, ApplianceEstimate
}
public enum FuelType
{
    Gasoline, Diesel, Electricity
}

// MODELS & ENTITIES
public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();
    public ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();
}

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public FuelType FuelType { get; set; }
    public UserProfile? UserProfile { get; set; }
    public ICollection<FuelPurchaseActivity> FuelPurchaseActivities { get; set; } = new List<FuelPurchaseActivity>();
}

public abstract class ActivityRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public ActivityUnit Unit { get; set; }
    public UserProfile? UserProfile { get; set; }
    public EmissionCalculation? EmissionCalculation { get; set; }
    public abstract decimal GetActivityValue();
}

public class FuelPurchaseActivity : ActivityRecord
{
    public Guid VehicleId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public FuelType FuelType { get; set; }
    public decimal LitersPurchased { get; set; }
    public decimal? TotalCost { get; set; }
    public Vehicle? Vehicle { get; set; }
    public FuelPurchaseActivity()
    {
        Unit = ActivityUnit.Liter;
    }
    public override decimal GetActivityValue()
    {
        return LitersPurchased;
    }
}

public class ElectricityUsage : ActivityRecord
{
    public ElectricityInputMethod InputMethod { get; set; }
    public decimal UsageKWh { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public ICollection<ApplianceUsage> ApplianceUsages { get; set; } = new List<ApplianceUsage>();
    public ElectricityUsage()
    {
        Unit = ActivityUnit.KilowattHour;
    }
    public override decimal GetActivityValue()
    {
        if (InputMethod == ElectricityInputMethod.ApplianceEstimate && ApplianceUsages.Count > 0)
        {
            return ApplianceUsages.Sum(a => a.EstimateKWh());
        }
        return UsageKWh;
    }
}

public class ApplianceUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ElectricityUsageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PowerWatt { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal HoursPerDay { get; set; }
    public int DaysUsed { get; set; }
    public ElectricityUsage? ElectricityUsage { get; set; }
    public decimal EstimateKWh()
    {
        return (PowerWatt / 1000m) * Quantity * HoursPerDay * DaysUsed;
    }
}

public class EmissionFactor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Value { get; set; }
    public ActivityUnit Unit { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public ICollection<EmissionCalculation> EmissionCalculations { get; set; } = new List<EmissionCalculation>();
}

public class EmissionCalculation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActivityRecordId { get; set; }
    public Guid EmissionFactorId { get; set; }
    public decimal ActivityValue { get; set; }
    public decimal ResultKgCO2e { get; set; }
    public ActivityRecord? ActivityRecord { get; set; }
    public EmissionFactor? EmissionFactor { get; set; }
}

public class Scenario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public DateTime BaselineStart { get; set; }
    public DateTime BaselineEnd { get; set; }
    public UserProfile? UserProfile { get; set; }
    public ICollection<ScenarioChange> ScenarioChanges { get; set; } = new List<ScenarioChange>();
    public ScenarioResult? ScenarioResult { get; set; }
}

public class ScenarioChange
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioId { get; set; }
    public ChangeType ChangeType { get; set; }
    public Guid TargetActivityRecordId { get; set; }
    public Guid? TargetApplianceUsageId { get; set; }
    public decimal ChangeValue { get; set; }
    public Scenario? Scenario { get; set; }
    public ActivityRecord? TargetActivityRecord { get; set; }
    public ApplianceUsage? TargetApplianceUsage { get; set; }
}

public class ScenarioResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioId { get; set; }
    public decimal BaselineKgCO2e { get; set; }
    public decimal ScenarioKgCO2e { get; set; }
    public decimal ReductionKgCO2e { get; set; }
    public Scenario? Scenario { get; set; }
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    public decimal? GetReductionPercentage()
    {
        if (BaselineKgCO2e <= 0)
        {
            return null;
        }
        return (ReductionKgCO2e / BaselineKgCO2e) * 100m;
    }
}

public class Recommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioResultId { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public decimal EstimatedReductionKgCO2e { get; set; }
    public int Priority { get; set; }

    
    public ScenarioResult? ScenarioResult { get; set; }
}

// SERVICES
public class EmissionCalculatorService
{
    public EmissionCalculation CalculateActivityEmission(ActivityRecord activity, EmissionFactor factor)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(factor);
        decimal activityValue = activity.GetActivityValue();
        decimal result = activityValue * factor.Value;
        return new EmissionCalculation
        {
            Id = Guid.NewGuid(),
            ActivityRecordId = activity.Id,
            EmissionFactorId = factor.Id,
            ActivityValue = activityValue,
            ResultKgCO2e = result,
            ActivityRecord = activity,
            EmissionFactor = factor
        };
    }
}

public class ScenarioService
{
    private readonly EmissionCalculatorService _calculatorService;
    public ScenarioService() : this(new EmissionCalculatorService())
    {
    }
    public ScenarioService(EmissionCalculatorService calculatorService)
    {
        _calculatorService = calculatorService ?? throw new ArgumentNullException(nameof(calculatorService));
    }
    public ScenarioResult RunScenario(Scenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        decimal baselineKg = 0m;
        decimal scenarioKg = 0m;
        foreach (var change in scenario.ScenarioChanges)
        {
            if (change.TargetActivityRecord?.EmissionCalculation != null)
            {
                decimal originalEmission = change.TargetActivityRecord.EmissionCalculation.ResultKgCO2e;
                baselineKg += originalEmission;

                decimal reductionFactor = Math.Clamp(change.ChangeValue, 0m, 100m) / 100m;
                decimal reducedEmission = originalEmission * (1m - reductionFactor);
                scenarioKg += reducedEmission;
            }
        }

        decimal reduction = baselineKg - scenarioKg;
        var result = new ScenarioResult
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenario.Id,
            BaselineKgCO2e = baselineKg,
            ScenarioKgCO2e = scenarioKg,
            ReductionKgCO2e = reduction,
            Scenario = scenario
        };
        result.Recommendations = GenerateRecommendations(result);
        return result;
    }

    public List<Recommendation> GenerateRecommendations(ScenarioResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var recommendations = new List<Recommendation>();
        if (result.Scenario != null)
        {
            int priority = 1;
            var orderedChanges = result.Scenario.ScenarioChanges
                .OrderByDescending(c => c.ChangeValue);

            foreach (var change in orderedChanges)
            {
                string actionDesc = change.ChangeType switch
                {
                    ChangeType.ReduceFuelPercent => $"Kurangi konsumsi bahan bakar sebesar {change.ChangeValue}%",
                    ChangeType.ReduceElectricityPercent => $"Kurangi penggunaan listrik sebesar {change.ChangeValue}%",
                    ChangeType.ReduceApplianceHoursPercent => $"Kurangi jam pemakaian alat elektronik sebesar {change.ChangeValue}%",
                    _ => "Kurangi emisi aktivitas"
                };

                recommendations.Add(new Recommendation
                {
                    Id = Guid.NewGuid(),
                    ScenarioResultId = result.Id,
                    RecommendedAction = actionDesc,
                    EstimatedReductionKgCO2e = result.ReductionKgCO2e,
                    Priority = priority++,
                    ScenarioResult = result
                });
            }
        }

        return recommendations;
    }
}
