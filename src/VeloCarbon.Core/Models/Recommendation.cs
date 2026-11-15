using System;

namespace VeloCarbon.Core.Models;

public class Recommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioResultId { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public decimal EstimatedReductionKgCO2e { get; set; }
    public int Priority { get; set; }
    public ScenarioResult? ScenarioResult { get; set; }
}
