using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Tests;

/// <summary>Penyimpanan in-memory untuk menguji service tanpa database.</summary>
public class InMemoryStore : IProfileRepository, IActivityRepository, IEmissionFactorRepository, IScenarioRepository, IUnitOfWork
{
    public List<UserProfile> Profiles = new();
    public List<Vehicle> Vehicles = new();
    public List<VehicleMonthlyInput> VehicleMonths = new();
    public List<ActivityRecord> Activities = new();
    public List<EmissionFactor> Factors = new();
    public List<Scenario> Scenarios = new();

    public Task<List<UserProfile>> GetAllAsync() => Task.FromResult(Profiles.ToList());
    Task<UserProfile?> IProfileRepository.GetAsync(Guid id) => Task.FromResult(Profiles.FirstOrDefault(p => p.Id == id));
    Task IProfileRepository.AddAsync(UserProfile p) { Profiles.Add(p); return Task.CompletedTask; }
    public Task<Vehicle?> GetVehicleAsync(Guid id) => Task.FromResult(Vehicles.FirstOrDefault(v => v.Id == id));
    public Task AddVehicleAsync(Vehicle v) { Vehicles.Add(v); return Task.CompletedTask; }

    Task IActivityRepository.AddAsync(ActivityRecord a) { Activities.Add(a); return Task.CompletedTask; }
    Task<ActivityRecord?> IActivityRepository.GetAsync(Guid id) => Task.FromResult(Activities.FirstOrDefault(a => a.Id == id));
    public Task<List<ActivityRecord>> GetByMonthAsync(Guid profileId, DateTime m) =>
        Task.FromResult(Activities.Where(a => a.UserProfileId == profileId && a.ReferenceDate >= m && a.ReferenceDate < m.AddMonths(1)).ToList());
    public Task<VehicleMonthlyInput?> GetVehicleMonthAsync(Guid v, DateTime m) => Task.FromResult(VehicleMonths.FirstOrDefault(x => x.VehicleId == v && x.Month == m));
    public Task AddVehicleMonthAsync(VehicleMonthlyInput i) { VehicleMonths.Add(i); return Task.CompletedTask; }
    public Task<ElectricityUsage?> GetElectricityAsync(Guid p, DateTime m) =>
        Task.FromResult(Activities.OfType<ElectricityUsage>().FirstOrDefault(e => e.UserProfileId == p && e.PeriodStart == m));
    public Task RemoveAsync(ActivityRecord a) { Activities.Remove(a); return Task.CompletedTask; }
    public Task<bool> VehicleMonthHasActivitiesAsync(Guid v, DateTime m, Guid exclude) => Task.FromResult(
        Activities.Where(a => a.Id != exclude && a.ReferenceDate >= m && a.ReferenceDate < m.AddMonths(1))
            .Any(a => (a is MobilityActivity ma && ma.VehicleId == v) || (a is FuelPurchaseActivity f && f.VehicleId == v)));
    public Task RemoveVehicleMonthAsync(Guid v, DateTime m) { VehicleMonths.RemoveAll(x => x.VehicleId == v && x.Month == m); return Task.CompletedTask; }

    Task<List<EmissionFactor>> IEmissionFactorRepository.GetAllAsync() => Task.FromResult(Factors.ToList());
    Task<EmissionFactor?> IEmissionFactorRepository.GetAsync(Guid id) => Task.FromResult(Factors.FirstOrDefault(f => f.Id == id));
    Task IEmissionFactorRepository.AddAsync(EmissionFactor f) { Factors.Add(f); return Task.CompletedTask; }

    Task IScenarioRepository.AddAsync(Scenario s) { Scenarios.Add(s); return Task.CompletedTask; }
    public Task<List<Scenario>> GetByProfileAsync(Guid p) => Task.FromResult(Scenarios.Where(s => s.UserProfileId == p).ToList());

    public Task SaveChangesAsync() => Task.CompletedTask;
}

public static class TestData
{
    /// <summary>Faktor sintetis dari PRD bagian 13 (bukan faktor produksi).</summary>
    public static List<EmissionFactor> SyntheticFactors() => new()
    {
        F(EmissionCategory.Mobility, "Motorcycle", 0.2m, ActivityUnit.Km),
        F(EmissionCategory.Mobility, "Car", 0.2m, ActivityUnit.Km),
        F(EmissionCategory.Mobility, "Bus", 0.1m, ActivityUnit.Km),
        F(EmissionCategory.Mobility, "Petrol", 2m, ActivityUnit.Liter),
        F(EmissionCategory.Electricity, "Grid", 0.5m, ActivityUnit.KWh),
    };
    public static EmissionFactor F(EmissionCategory c, string sub, decimal v, ActivityUnit u) => new()
    { Category = c, Subcategory = sub, Value = v, Unit = u, Source = "sintetis-uji", Version = "t1", ValidFrom = new DateTime(2020, 1, 1) };
}
