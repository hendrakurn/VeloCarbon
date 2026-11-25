using Microsoft.EntityFrameworkCore;
using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;
using VeloCarbon.Infrastructure.Persistence;

namespace VeloCarbon.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly VeloCarbonDbContext _db;
    public ProfileRepository(VeloCarbonDbContext db) => _db = db;
    public Task<List<UserProfile>> GetAllAsync() => _db.Profiles.Include(p => p.Vehicles).OrderBy(p => p.CreatedAt).ToListAsync();
    public Task<UserProfile?> GetAsync(Guid id) => _db.Profiles.Include(p => p.Vehicles).FirstOrDefaultAsync(p => p.Id == id);
    public async Task AddAsync(UserProfile profile) => await _db.Profiles.AddAsync(profile);
    public Task<Vehicle?> GetVehicleAsync(Guid id) => _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
    public async Task AddVehicleAsync(Vehicle v) => await _db.Vehicles.AddAsync(v);
}

public class ActivityRepository : IActivityRepository
{
    private readonly VeloCarbonDbContext _db;
    public ActivityRepository(VeloCarbonDbContext db) => _db = db;

    private IQueryable<ActivityRecord> Full() => _db.Activities
        .Include(a => a.Calculation).ThenInclude(c => c!.EmissionFactor)
        .Include(a => ((ElectricityUsage)a).Appliances);

    public async Task AddAsync(ActivityRecord a) => await _db.Activities.AddAsync(a);
    public Task<ActivityRecord?> GetAsync(Guid id) => Full().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<ActivityRecord>> GetByMonthAsync(Guid profileId, DateTime monthStart)
    {
        var next = monthStart.AddMonths(1);
        var all = await Full().Where(a => a.UserProfileId == profileId).ToListAsync();
        // ReferenceDate adalah properti turunan (tidak dipetakan), sehingga dipilah di memori.
        return all.Where(a => a.ReferenceDate >= monthStart && a.ReferenceDate < next).OrderBy(a => a.ReferenceDate).ToList();
    }

    public Task<VehicleMonthlyInput?> GetVehicleMonthAsync(Guid vehicleId, DateTime m) =>
        _db.VehicleMonthlyInputs.FirstOrDefaultAsync(x => x.VehicleId == vehicleId && x.Month == m);
    public async Task AddVehicleMonthAsync(VehicleMonthlyInput i) => await _db.VehicleMonthlyInputs.AddAsync(i);
    public async Task<ElectricityUsage?> GetElectricityAsync(Guid profileId, DateTime m) =>
        await _db.Activities.OfType<ElectricityUsage>().FirstOrDefaultAsync(e => e.UserProfileId == profileId && e.PeriodStart == m);
    public Task RemoveAsync(ActivityRecord a) { _db.Activities.Remove(a); return Task.CompletedTask; }

    public async Task<bool> VehicleMonthHasActivitiesAsync(Guid vehicleId, DateTime m, Guid excludeId)
    {
        var next = m.AddMonths(1);
        var mob = await _db.Activities.OfType<MobilityActivity>().AnyAsync(x => x.VehicleId == vehicleId && x.Id != excludeId && x.ActivityDate >= m && x.ActivityDate < next);
        if (mob) return true;
        return await _db.Activities.OfType<FuelPurchaseActivity>().AnyAsync(x => x.VehicleId == vehicleId && x.Id != excludeId && x.PurchaseDate >= m && x.PurchaseDate < next);
    }

    public async Task RemoveVehicleMonthAsync(Guid vehicleId, DateTime m)
    {
        var row = await _db.VehicleMonthlyInputs.FirstOrDefaultAsync(x => x.VehicleId == vehicleId && x.Month == m);
        if (row != null) _db.VehicleMonthlyInputs.Remove(row);
    }
}

public class EmissionFactorRepository : IEmissionFactorRepository
{
    private readonly VeloCarbonDbContext _db;
    public EmissionFactorRepository(VeloCarbonDbContext db) => _db = db;
    public Task<List<EmissionFactor>> GetAllAsync() => _db.EmissionFactors.OrderBy(f => f.Category).ThenBy(f => f.Subcategory).ThenBy(f => f.ValidFrom).ToListAsync();
    public Task<EmissionFactor?> GetAsync(Guid id) => _db.EmissionFactors.FirstOrDefaultAsync(f => f.Id == id);
    public async Task AddAsync(EmissionFactor f) => await _db.EmissionFactors.AddAsync(f);
}

public class ScenarioRepository : IScenarioRepository
{
    private readonly VeloCarbonDbContext _db;
    public ScenarioRepository(VeloCarbonDbContext db) => _db = db;
    public async Task AddAsync(Scenario s) => await _db.Scenarios.AddAsync(s);
    public Task<List<Scenario>> GetByProfileAsync(Guid profileId) => _db.Scenarios
        .Include(s => s.Changes).Include(s => s.Result).ThenInclude(r => r!.Recommendations)
        .Where(s => s.UserProfileId == profileId).OrderByDescending(s => s.CreatedAt).ToListAsync();
}
