using System;
using System.Collections.Generic;
using System.Linq;
using VeloCarbon.Core.Enums;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

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
