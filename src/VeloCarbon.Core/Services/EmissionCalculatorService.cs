using System;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

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
