using VeloCarbon.Core.Enums;
using VeloCarbon.Core.Models;
using VeloCarbon.Data;
using Xunit;

namespace VeloCarbon.Tests;

// Butuh PostgreSQL dari docker compose dan env VELOCARBON_DB.
public class DatabaseIntegrationTests
{
    [Fact]
    public async Task EmissionFactor_crud_and_fuel_purchase_roundtrip()
    {
        await using var db = VeloCarbonDbContext.FromEnvironment();
        await DemoSeed.EnsureProfileAndVehicleAsync(db);
        var factors = new EmissionFactorRepository(db);
        var fuels = new FuelPurchaseRepository(db);

        var factor = new EmissionFactor { Value = 2.0m, Unit = ActivityUnit.Liter, Source = "Test", Version = "test-" + Guid.NewGuid() };
        await factors.AddAsync(factor, "Mobility", "Gasoline", new DateOnly(2026, 1, 1));
        Assert.Equal(2.0m, (await factors.GetByIdAsync(factor.Id))!.Value);

        factor.Value = 2.5m;
        Assert.True(await factors.UpdateAsync(factor));
        Assert.Equal(2.5m, (await factors.GetByIdAsync(factor.Id))!.Value);

        var fuel = new FuelPurchaseActivity
        {
            UserProfileId = DemoSeed.ProfileId,
            VehicleId = DemoSeed.VehicleId,
            PurchaseDate = new DateTime(2026, 11, 1),
            FuelType = FuelType.Gasoline,
            LitersPurchased = 4m
        };
        var calc = await fuels.AddWithEmissionAsync(fuel, factor);
        Assert.Equal(10m, calc.ResultKgCO2e);

        var saved = (await fuels.GetAllAsync()).Single(a => a.Id == fuel.Id);
        Assert.Equal(4m, saved.LitersPurchased);
        Assert.Equal(10m, saved.EmissionCalculation!.ResultKgCO2e);

        Assert.True(await fuels.DeleteAsync(fuel.Id));
        Assert.True(await factors.DeleteAsync(factor.Id));
        Assert.Null(await factors.GetByIdAsync(factor.Id));
    }
}
