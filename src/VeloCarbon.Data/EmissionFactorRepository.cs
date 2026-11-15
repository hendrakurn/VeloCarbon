using Microsoft.EntityFrameworkCore;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Data;

public class EmissionFactorRepository
{
    private readonly VeloCarbonDbContext _db;

    public EmissionFactorRepository(VeloCarbonDbContext db) => _db = db;

    public async Task AddAsync(EmissionFactor factor, string category, string subcategory, DateOnly validFrom)
    {
        _db.EmissionFactors.Add(factor);
        var entry = _db.Entry(factor);
        entry.Property("Category").CurrentValue = category;
        entry.Property("Subcategory").CurrentValue = subcategory;
        entry.Property("ValidFrom").CurrentValue = validFrom;
        entry.Property("IsActive").CurrentValue = true;
        await _db.SaveChangesAsync();
    }

    public Task<List<EmissionFactor>> GetAllAsync() =>
        _db.EmissionFactors.AsNoTracking().OrderBy(f => f.Version).ToListAsync();

    public Task<EmissionFactor?> GetByIdAsync(Guid id) =>
        _db.EmissionFactors.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

    // Hanya property model yang disalin; shadow property (category, dst.) tidak tertimpa.
    public async Task<bool> UpdateAsync(EmissionFactor factor)
    {
        var existing = await _db.EmissionFactors.FindAsync(factor.Id);
        if (existing is null)
        {
            return false;
        }

        _db.Entry(existing).CurrentValues.SetValues(factor);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _db.EmissionFactors.FindAsync(id);
        if (existing is null)
        {
            return false;
        }

        _db.EmissionFactors.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
