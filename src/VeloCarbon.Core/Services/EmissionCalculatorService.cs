using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public interface IEmissionCalculatorService
{
    EmissionCalculation CalculateActivityEmission(ActivityRecord activity, EmissionFactor factor);
}

/// <summary>Murni (tanpa database/UI): emisi = nilai aktivitas x faktor emisi. Tidak ada pembulatan internal.</summary>
public class EmissionCalculatorService : IEmissionCalculatorService
{
    public EmissionCalculation CalculateActivityEmission(ActivityRecord activity, EmissionFactor factor)
    {
        if (factor.Value < 0) throw new DomainException("Nilai faktor emisi tidak boleh negatif.");
        if (factor.Unit != activity.Unit)
            throw new DomainException($"Satuan faktor ({factor.Unit}) tidak cocok dengan satuan aktivitas ({activity.Unit}).");
        if (factor.Category != activity.Category || !string.Equals(factor.Subcategory, activity.FactorSubcategory, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"Faktor emisi {factor.Category}/{factor.Subcategory} tidak cocok untuk aktivitas {activity.Category}/{activity.FactorSubcategory}.");

        var value = activity.GetActivityValue();
        return new EmissionCalculation
        {
            ActivityRecordId = activity.Id,
            EmissionFactorId = factor.Id,
            EmissionFactor = factor,
            ActivityValue = value,
            ResultKgCO2e = value * factor.Value,
            CalculatedAt = DateTime.UtcNow
        };
    }
}
