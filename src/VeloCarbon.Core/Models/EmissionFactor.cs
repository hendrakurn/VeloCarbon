using System;
using System.Collections.Generic;
using VeloCarbon.Core.Enums;

namespace VeloCarbon.Core.Models;

public class EmissionFactor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Value { get; set; }
    public ActivityUnit Unit { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    // Navigation property
    public ICollection<EmissionCalculation> EmissionCalculations { get; set; } = new List<EmissionCalculation>();
}
