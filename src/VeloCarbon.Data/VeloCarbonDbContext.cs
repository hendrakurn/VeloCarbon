using Microsoft.EntityFrameworkCore;
using VeloCarbon.Core.Enums;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Data;

// Memetakan model domain ke skema db/init.sql (database sudah ada, tanpa migration).
// Kolom DB yang belum ada di model domain dipetakan sebagai shadow property
// supaya kontrak model Core tidak berubah.
public class VeloCarbonDbContext : DbContext
{
    public const string ConnectionStringVariable = "VELOCARBON_DB";

    public VeloCarbonDbContext(string connectionString)
        : base(new DbContextOptionsBuilder<VeloCarbonDbContext>().UseNpgsql(connectionString).Options)
    {
    }

    public DbSet<EmissionFactor> EmissionFactors => Set<EmissionFactor>();
    public DbSet<FuelPurchaseActivity> FuelPurchaseActivities => Set<FuelPurchaseActivity>();

    public static VeloCarbonDbContext FromEnvironment()
    {
        string? connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Environment variable {ConnectionStringVariable} belum diisi connection string PostgreSQL.");
        }

        return new VeloCarbonDbContext(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ponytail: konversi satuan hanya liter & kWh (model belum punya km/Mobility).
        // Tambahkan nilai enum + cabang konversi saat aktivitas jarak masuk ke model.
        modelBuilder.Entity<EmissionFactor>(e =>
        {
            e.ToTable("emission_factors");
            e.Property(f => f.Id).HasColumnName("id");
            e.Property(f => f.Value).HasColumnName("value");
            e.Property(f => f.Unit).HasColumnName("unit").HasConversion(
                u => u == ActivityUnit.Liter ? "kgCO2e/liter" : "kgCO2e/kWh",
                s => s == "kgCO2e/liter" ? ActivityUnit.Liter : ActivityUnit.KilowattHour);
            e.Property(f => f.Source).HasColumnName("source");
            e.Property(f => f.Version).HasColumnName("version");
            e.Property<string>("Category").HasColumnName("category");
            e.Property<string>("Subcategory").HasColumnName("subcategory");
            e.Property<DateOnly>("ValidFrom").HasColumnName("valid_from");
            e.Property<DateOnly?>("ValidUntil").HasColumnName("valid_until");
            e.Property<bool>("IsActive").HasColumnName("is_active");
        });

        // Table-per-type: activity_records (induk) + fuel_purchase_activities (anak).
        modelBuilder.Entity<ActivityRecord>(e =>
        {
            e.ToTable("activity_records");
            e.UseTptMappingStrategy();
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.UserProfileId).HasColumnName("user_profile_id");
            e.Property(a => a.Unit).HasColumnName("unit").HasConversion(
                u => u == ActivityUnit.Liter ? "liter" : "kWh",
                s => s == "liter" ? ActivityUnit.Liter : ActivityUnit.KilowattHour);
            e.Property<string>("ActivityType").HasColumnName("activity_type");
            e.Ignore(a => a.UserProfile);
        });

        modelBuilder.Entity<FuelPurchaseActivity>(e =>
        {
            e.ToTable("fuel_purchase_activities",
                t => t.Property(a => a.Id).HasColumnName("activity_record_id"));
            e.Property(a => a.VehicleId).HasColumnName("vehicle_id");
            e.Property(a => a.PurchaseDate).HasColumnName("purchase_date").HasColumnType("date");
            e.Property(a => a.FuelType).HasColumnName("fuel_type").HasConversion<string>();
            e.Property(a => a.LitersPurchased).HasColumnName("liters_purchased");
            e.Property(a => a.TotalCost).HasColumnName("total_cost");
            e.Ignore(a => a.Vehicle);
        });

        modelBuilder.Ignore<ElectricityUsage>();

        modelBuilder.Entity<EmissionCalculation>(e =>
        {
            e.ToTable("emission_calculations");
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.ActivityRecordId).HasColumnName("activity_record_id");
            e.Property(c => c.EmissionFactorId).HasColumnName("emission_factor_id");
            e.Property(c => c.ActivityValue).HasColumnName("activity_value");
            e.Property(c => c.ResultKgCO2e).HasColumnName("result_kg_co2e");
            e.HasOne(c => c.ActivityRecord).WithOne(a => a.EmissionCalculation)
                .HasForeignKey<EmissionCalculation>(c => c.ActivityRecordId);
            e.HasOne(c => c.EmissionFactor).WithMany(f => f.EmissionCalculations)
                .HasForeignKey(c => c.EmissionFactorId);
        });
    }
}
