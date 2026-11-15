using Microsoft.EntityFrameworkCore;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Data;

public class FuelPurchaseRepository
{
    private readonly VeloCarbonDbContext _db;

    public FuelPurchaseRepository(VeloCarbonDbContext db) => _db = db;

    // Satu SaveChanges = satu transaksi: activity_records, fuel_purchase_activities,
    // dan emission_calculations tersimpan bersama atau dibatalkan bersama.
    public async Task<EmissionCalculation> AddWithEmissionAsync(FuelPurchaseActivity activity, EmissionFactor factor)
    {
        activity.EmissionCalculation = new EmissionCalculation
        {
            ActivityRecordId = activity.Id,
            EmissionFactorId = factor.Id,
            ActivityValue = activity.GetActivityValue(),
            ResultKgCO2e = activity.CalculateEmission(factor)
        };

        _db.FuelPurchaseActivities.Add(activity);
        _db.Entry(activity).Property("ActivityType").CurrentValue = "FuelPurchase";
        await _db.SaveChangesAsync();
        return activity.EmissionCalculation;
    }

    public Task<List<FuelPurchaseActivity>> GetAllAsync() =>
        _db.FuelPurchaseActivities.AsNoTracking()
            .Include(a => a.EmissionCalculation)
            .OrderBy(a => a.PurchaseDate)
            .ToListAsync();

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _db.FuelPurchaseActivities.FindAsync(id);
        if (existing is null)
        {
            return false;
        }

        _db.FuelPurchaseActivities.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
