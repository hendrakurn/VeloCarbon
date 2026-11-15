using System;
using VeloCarbon.Core.Enums;

namespace VeloCarbon.Core.Models;

public abstract class ActivityRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public ActivityUnit Unit { get; set; }
    public UserProfile? UserProfile { get; set; }
    public EmissionCalculation? EmissionCalculation { get; set; }
    public abstract decimal GetActivityValue();

    // Template method: rumus sama untuk semua aktivitas, nilai aktivitas
    // diambil secara polymorphic dari kelas turunan (liter / kWh).
    public decimal CalculateEmission(EmissionFactor factor)
    {
        ArgumentNullException.ThrowIfNull(factor);
        if (factor.Unit != Unit)
        {
            throw new InvalidOperationException(
                $"Satuan faktor emisi ({factor.Unit}) tidak cocok dengan satuan aktivitas ({Unit}).");
        }

        return GetActivityValue() * factor.Value;
    }
}
