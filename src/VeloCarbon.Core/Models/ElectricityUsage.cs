using System;
using System.Collections.Generic;
using System.Linq;
using VeloCarbon.Core.Enums;

namespace VeloCarbon.Core.Models;

public class ElectricityUsage : ActivityRecord
{
    public ElectricityInputMethod InputMethod { get; set; }
    private decimal _usageKWh;
    public decimal UsageKWh
    {
        get => _usageKWh;
        set => _usageKWh = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(UsageKWh), value, "Pemakaian kWh tidak boleh negatif.");
    }
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
