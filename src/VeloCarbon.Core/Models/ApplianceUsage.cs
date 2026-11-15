using System;

namespace VeloCarbon.Core.Models;

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
