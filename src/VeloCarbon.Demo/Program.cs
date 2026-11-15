using Microsoft.EntityFrameworkCore;
using Npgsql;
using VeloCarbon.Core.Enums;
using VeloCarbon.Core.Models;
using VeloCarbon.Data;

// Demo Homework 8: CRUD faktor emisi + catat pembelian BBM ke PostgreSQL.
// Tiap langkah berhenti menunggu Enter supaya data bisa dicek/di-screenshot di pgAdmin.

await using var db = VeloCarbonDbContext.FromEnvironment();
var factors = new EmissionFactorRepository(db);
var fuels = new FuelPurchaseRepository(db);

Step("0. KONEKSI");
await DemoSeed.EnsureProfileAndVehicleAsync(db);
Console.WriteLine($"Terhubung ke database: {db.Database.GetDbConnection().Database}");

Step("1. CREATE faktor emisi");
var factor = new EmissionFactor { Value = 2.31m, Unit = ActivityUnit.Liter, Source = "IPCC 2006 (contoh)", Version = "hw8-v1" };
await factors.AddAsync(factor, "Mobility", "Gasoline", new DateOnly(2026, 1, 1));
Console.WriteLine($"Tersimpan: {factor.Id}");
Pause();

Step("2. READ semua faktor emisi");
PrintFactors(await factors.GetAllAsync());
Pause();

Step("3. UPDATE faktor emisi");
factor.Value = 2.30m;
factor.Version = "hw8-v2";
Console.WriteLine(await factors.UpdateAsync(factor) ? "Update berhasil" : "Data tidak ditemukan");
PrintFactors(await factors.GetAllAsync());
Pause();

Step("4. CREATE pembelian BBM + hitung emisi (polymorphism, satu transaksi)");
var fuel = new FuelPurchaseActivity
{
    UserProfileId = DemoSeed.ProfileId,
    VehicleId = DemoSeed.VehicleId,
    PurchaseDate = DateTime.Today,
    FuelType = FuelType.Gasoline,
    LitersPurchased = 10m,
    TotalCost = 100000m
};
var calc = await fuels.AddWithEmissionAsync(fuel, factor);
Console.WriteLine($"{calc.ActivityValue} liter x {factor.Value} = {calc.ResultKgCO2e} kgCO2e");
Pause();

Step("5. READ pembelian BBM");
foreach (var f in await fuels.GetAllAsync())
{
    Console.WriteLine($"{f.Id} | {f.PurchaseDate:yyyy-MM-dd} | {f.FuelType} | {f.LitersPurchased} L | {f.EmissionCalculation?.ResultKgCO2e} kgCO2e");
}
Pause();

Step("6. DELETE pembelian BBM lalu faktor emisi");
Console.WriteLine($"Hapus BBM: {(await fuels.DeleteAsync(fuel.Id) ? "berhasil" : "tidak ditemukan")}");
Console.WriteLine($"Hapus faktor: {(await factors.DeleteAsync(factor.Id) ? "berhasil" : "tidak ditemukan")}");
PrintFactors(await factors.GetAllAsync());
Pause();

Step("7. KASUS GAGAL (ditangani try/catch)");
try
{
    _ = new FuelPurchaseActivity { LitersPurchased = -5m };
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"[VALIDASI GAGAL] {ex.Message}");
}

try
{
    var wrong = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString()) { Password = "password-salah" };
    await using var badDb = new VeloCarbonDbContext(wrong.ConnectionString);
    await badDb.Database.OpenConnectionAsync();
}
catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
{
    Console.WriteLine($"[KONEKSI GAGAL] {ex.Message}");
}

Console.WriteLine("\nDemo selesai.");

static void Step(string title) => Console.WriteLine($"\n===== {title} =====");

// Input kosong (< /dev/null) membuat ReadLine langsung kembali, jadi demo jalan tanpa jeda.
static void Pause()
{
    Console.WriteLine("(Enter untuk lanjut)");
    Console.ReadLine();
}

static void PrintFactors(List<EmissionFactor> list)
{
    Console.WriteLine($"{list.Count} faktor emisi:");
    foreach (var f in list)
    {
        Console.WriteLine($"  {f.Id} | {f.Value} {f.Unit} | {f.Source} | {f.Version}");
    }
}
