using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Abstractions;

public interface IProfileRepository
{
    Task<List<UserProfile>> GetAllAsync();
    Task<UserProfile?> GetAsync(Guid id);
    Task AddAsync(UserProfile profile);
    Task<Vehicle?> GetVehicleAsync(Guid vehicleId);
    Task AddVehicleAsync(Vehicle vehicle);
}

public interface IActivityRepository
{
    Task AddAsync(ActivityRecord activity);
    Task<ActivityRecord?> GetAsync(Guid id);
    /// <summary>Aktivitas profil pada bulan kalender tertentu (lengkap dengan perhitungan & rincian alat).</summary>
    Task<List<ActivityRecord>> GetByMonthAsync(Guid profileId, DateTime monthStart);
    Task<VehicleMonthlyInput?> GetVehicleMonthAsync(Guid vehicleId, DateTime monthStart);
    Task AddVehicleMonthAsync(VehicleMonthlyInput input);
    Task<ElectricityUsage?> GetElectricityAsync(Guid profileId, DateTime monthStart);
    Task RemoveAsync(ActivityRecord activity);
    Task<bool> VehicleMonthHasActivitiesAsync(Guid vehicleId, DateTime monthStart, Guid excludeActivityId);
    Task RemoveVehicleMonthAsync(Guid vehicleId, DateTime monthStart);
}

public interface IEmissionFactorRepository
{
    Task<List<EmissionFactor>> GetAllAsync();
    Task<EmissionFactor?> GetAsync(Guid id);
    Task AddAsync(EmissionFactor factor);
}

public interface IScenarioRepository
{
    Task AddAsync(Scenario scenario);
    Task<List<Scenario>> GetByProfileAsync(Guid profileId);
}

/// <summary>Satu SaveChanges = satu transaksi atomik (aktivitas + hasil emisi tidak pernah terpisah).</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
