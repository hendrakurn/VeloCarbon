using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>Menerapkan migrasi, mengisi faktor emisi awal, dan (opsional) data contoh. Aman dipanggil berulang.</summary>
    public static async Task InitializeAsync(IServiceProvider sp, bool seedDemoData)
    {
        var db = sp.GetRequiredService<VeloCarbonDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.EmissionFactors.AnyAsync())
        {
            db.EmissionFactors.AddRange(DefaultFactors());
            await db.SaveChangesAsync();
        }

        if (seedDemoData && !await db.Profiles.AnyAsync())
            await SeedDemoAsync(sp);
    }

    private const string EduNote = "Nilai edukatif (estimasi), verifikasi ke sumber sebelum dipakai di luar pembelajaran.";

    public static List<EmissionFactor> DefaultFactors()
    {
        var from = new DateTime(2024, 1, 1);
        EmissionFactor F(EmissionCategory c, string sub, decimal v, ActivityUnit u, string src) => new()
        { Category = c, Subcategory = sub, Value = v, Unit = u, Source = $"{src}. {EduNote}", Version = "2024.1", ValidFrom = from };
        return new()
        {
            F(EmissionCategory.Mobility, "Walk", 0m, ActivityUnit.Km, "Tanpa emisi langsung"),
            F(EmissionCategory.Mobility, "Bicycle", 0m, ActivityUnit.Km, "Tanpa emisi langsung"),
            F(EmissionCategory.Mobility, "Bus", 0.089m, ActivityUnit.Km, "Referensi faktor moda (DEFRA/IPCC), per penumpang-km"),
            F(EmissionCategory.Mobility, "Train", 0.041m, ActivityUnit.Km, "Referensi faktor moda (DEFRA/IPCC), per penumpang-km"),
            F(EmissionCategory.Mobility, "Motorcycle", 0.103m, ActivityUnit.Km, "Referensi faktor moda (DEFRA/IPCC)"),
            F(EmissionCategory.Mobility, "Car", 0.192m, ActivityUnit.Km, "Referensi faktor moda (DEFRA/IPCC)"),
            F(EmissionCategory.Mobility, "Petrol", 2.31m, ActivityUnit.Liter, "IPCC 2006 Guidelines, pembakaran bensin"),
            F(EmissionCategory.Mobility, "Diesel", 2.68m, ActivityUnit.Liter, "IPCC 2006 Guidelines, pembakaran solar"),
            F(EmissionCategory.Electricity, "Grid", 0.87m, ActivityUnit.KWh, "Faktor emisi jaringan listrik Indonesia (KESDM/PLN)"),
        };
    }

    private static async Task SeedDemoAsync(IServiceProvider sp)
    {
        var profiles = sp.GetRequiredService<ProfileService>();
        var activities = sp.GetRequiredService<ActivityService>();
        var p = await profiles.CreateProfileAsync("Mahasiswa Demo");
        var motor = await profiles.AddVehicleAsync(p.Id, "Motor Harian", TransportMode.Motorcycle, FuelType.Petrol);
        var today = DateTime.Today;
        // 3 bulan terakhir: tren + data untuk simulasi.
        for (var back = 2; back >= 0; back--)
        {
            var month = new DateTime(today.Year, today.Month, 1).AddMonths(-back);
            await activities.AddDistanceAsync(new DistanceInput(p.Id, motor.Id, month.AddDays(4), TransportMode.Motorcycle, 8m + back, 20));
            await activities.AddDistanceAsync(new DistanceInput(p.Id, null, month.AddDays(9), TransportMode.Bus, 12m, 4));
            if (back == 0)
                await activities.AddApplianceElectricityAsync(new ApplianceElectricityInput(p.Id, month, new[]
                {
                    new ApplianceInput("Laptop", 65m, 1, 8m, 26),
                    new ApplianceInput("Kipas Angin", 45m, 1, 10m, 30),
                    new ApplianceInput("Lampu LED", 9m, 2, 6m, 30),
                }));
            else
                await activities.AddMeterElectricityAsync(new MeterElectricityInput(p.Id, month, 60m + back * 5m));
        }
    }
}
