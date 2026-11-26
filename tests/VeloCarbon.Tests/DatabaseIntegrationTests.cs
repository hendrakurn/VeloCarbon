using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;
using VeloCarbon.Infrastructure;
using VeloCarbon.Infrastructure.Persistence;

namespace VeloCarbon.Tests;

public sealed class FactIfDbAttribute : FactAttribute
{
    public FactIfDbAttribute() { if (Environment.GetEnvironmentVariable("VELOCARBON_TEST_DB") is null) Skip = "Set VELOCARBON_TEST_DB ke connection string PostgreSQL untuk menjalankan tes integrasi."; }
}

/// <summary>Tes integrasi ke PostgreSQL sungguhan (database dibuat baru per tes, dihapus setelahnya).</summary>
public class DatabaseIntegrationTests
{
    private static ServiceProvider Build(out string dbName)
    {
        dbName = "vc_test_" + Guid.NewGuid().ToString("N")[..10];
        var baseCs = Environment.GetEnvironmentVariable("VELOCARBON_TEST_DB")!;
        var cs = new Npgsql.NpgsqlConnectionStringBuilder(baseCs) { Database = dbName }.ToString();
        var services = new ServiceCollection().AddVeloCarbon(cs).BuildServiceProvider();
        return services;
    }

    private static async Task Drop(ServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<VeloCarbonDbContext>().Database.EnsureDeletedAsync();
    }

    [FactIfDb] public async Task Data_survives_reopen_and_all_four_methods_work()
    {
        var sp = Build(out _);
        try
        {
            Guid profileId, motorId;
            using (var scope = sp.CreateScope())
            {
                await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, seedDemoData: false);
                var pr = scope.ServiceProvider.GetRequiredService<ProfileService>();
                var act = scope.ServiceProvider.GetRequiredService<ActivityService>();
                var p = await pr.CreateProfileAsync("Integrasi"); profileId = p.Id;
                motorId = (await pr.AddVehicleAsync(p.Id, "Motor", TransportMode.Motorcycle, FuelType.Petrol)).Id;
                var car = await pr.AddVehicleAsync(p.Id, "Mobil", TransportMode.Car, FuelType.Petrol);
                var m = new DateTime(2026, 9, 1);
                await act.AddDistanceAsync(new(p.Id, motorId, m.AddDays(1), TransportMode.Motorcycle, 10, 2));
                await act.AddFuelPurchaseAsync(new(p.Id, car.Id, m.AddDays(2), FuelType.Petrol, 5, 75000));
                await act.AddApplianceElectricityAsync(new(p.Id, m, new[] { new ApplianceInput("AC", 100, 2, 5, 10) }));
                await Assert.ThrowsAsync<DomainException>(() => act.AddFuelPurchaseAsync(new(p.Id, motorId, m.AddDays(3), FuelType.Petrol, 3, null)));
            }
            using (var scope = sp.CreateScope()) // "aplikasi dibuka ulang"
            {
                var act = scope.ServiceProvider.GetRequiredService<ActivityService>();
                var list = await act.GetMonthAsync(profileId, new DateTime(2026, 9, 1));
                Assert.Equal(3, list.Count);
                Assert.All(list, a => Assert.NotNull(a.Calculation));
                var elec = list.OfType<ElectricityUsage>().Single();
                Assert.Equal(10m, elec.UsageKWh);
                Assert.Single(elec.Appliances);
                var dash = await scope.ServiceProvider.GetRequiredService<DashboardService>().GetSummaryAsync(profileId, new DateTime(2026, 9, 1));
                Assert.True(dash.TotalKgCO2e > 0);
            }
        }
        finally { await Drop(sp); }
    }

    [FactIfDb] public async Task Database_blocks_duplicate_electricity_for_same_profile_month()
    {
        var sp = Build(out _);
        try
        {
            using var scope = sp.CreateScope();
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, false);
            var db = scope.ServiceProvider.GetRequiredService<VeloCarbonDbContext>();
            var p = await scope.ServiceProvider.GetRequiredService<ProfileService>().CreateProfileAsync("X");
            var start = new DateTime(2026, 9, 1);
            db.Activities.Add(new ElectricityUsage { UserProfileId = p.Id, UsageKWh = 1, PeriodStart = start, PeriodEnd = start.AddDays(29) });
            db.Activities.Add(new ElectricityUsage { UserProfileId = p.Id, UsageKWh = 2, PeriodStart = start, PeriodEnd = start.AddDays(29) });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
        finally { await Drop(sp); }
    }

    [FactIfDb] public async Task Seed_creates_factors_and_demo_data_idempotently()
    {
        var sp = Build(out _);
        try
        {
            using var scope = sp.CreateScope();
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, true);
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, true);
            var db = scope.ServiceProvider.GetRequiredService<VeloCarbonDbContext>();
            Assert.Equal(9, await db.EmissionFactors.CountAsync());
            Assert.Equal(1, await db.Profiles.CountAsync());
            Assert.True(await db.Activities.CountAsync() >= 7);
        }
        finally { await Drop(sp); }
    }

    [FactIfDb] public async Task Scenario_is_persisted_with_recommendations()
    {
        var sp = Build(out _);
        try
        {
            using var scope = sp.CreateScope();
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, true);
            var db = scope.ServiceProvider.GetRequiredService<VeloCarbonDbContext>();
            var profile = await db.Profiles.FirstAsync();
            var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var act = scope.ServiceProvider.GetRequiredService<ActivityService>();
            var elec = (await act.GetMonthAsync(profile.Id, month)).OfType<ElectricityUsage>().Single();
            var scn = await scope.ServiceProvider.GetRequiredService<ScenarioService>().RunAndSaveAsync(
                new(profile.Id, "Hemat 10%", month, new[] { new ScenarioChangeInput(ChangeType.ReduceApplianceHours, elec.Id, elec.Appliances[0].Id, 10) }));
            var loaded = (await scope.ServiceProvider.GetRequiredService<ScenarioService>().GetByProfileAsync(profile.Id)).Single();
            Assert.Equal(scn.Result!.ReductionKgCO2e, loaded.Result!.ReductionKgCO2e);
            Assert.Single(loaded.Result.Recommendations);
        }
        finally { await Drop(sp); }
    }

    [FactIfDb] public async Task Profile_and_vehicle_crud_and_activity_reference_protection_work_in_postgres()
    {
        var sp = Build(out _);
        try
        {
            using var scope = sp.CreateScope();
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, false);
            var pr = scope.ServiceProvider.GetRequiredService<ProfileService>();
            var act = scope.ServiceProvider.GetRequiredService<ActivityService>();

            var p = await pr.CreateProfileAsync("Awal");
            await pr.UpdateProfileAsync(p.Id, "Diperbarui");
            var v = await pr.AddVehicleAsync(p.Id, "Motor 1", TransportMode.Motorcycle, FuelType.Petrol);
            await pr.UpdateVehicleAsync(p.Id, v.Id, "Motor Utama", TransportMode.Motorcycle, FuelType.Petrol);

            var a = await act.AddDistanceAsync(new(p.Id, v.Id, new DateTime(2026, 9, 5), TransportMode.Motorcycle, 10, 2));
            await Assert.ThrowsAsync<DomainException>(() => pr.DeleteVehicleAsync(p.Id, v.Id));
            await Assert.ThrowsAsync<DomainException>(() => pr.UpdateVehicleAsync(p.Id, v.Id, "Motor Utama", TransportMode.Car, FuelType.Petrol));

            await act.DeleteAsync(a.Id);
            await pr.DeleteVehicleAsync(p.Id, v.Id);

            var reloaded = await pr.GetAsync(p.Id);
            Assert.Equal("Diperbarui", reloaded!.Name);
            Assert.Empty(reloaded.Vehicles);
        }
        finally { await Drop(sp); }
    }
}
