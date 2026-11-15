using System;

namespace VeloCarbon.Core.Models;

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
