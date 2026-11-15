using System;
using System.Collections.Generic;

namespace VeloCarbon.Core.Models;

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
