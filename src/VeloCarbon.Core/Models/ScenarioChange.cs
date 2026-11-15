using System;
using VeloCarbon.Core.Enums;

namespace VeloCarbon.Core.Models;

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
