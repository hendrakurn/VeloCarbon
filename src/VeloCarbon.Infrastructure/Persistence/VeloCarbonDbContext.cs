using Microsoft.EntityFrameworkCore;
using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Infrastructure.Persistence;

public class VeloCarbonDbContext : DbContext, IUnitOfWork
{
    public VeloCarbonDbContext(DbContextOptions<VeloCarbonDbContext> options) : base(options) { }

    public DbSet<UserProfile> Profiles => Set<UserProfile>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleMonthlyInput> VehicleMonthlyInputs => Set<VehicleMonthlyInput>();
    public DbSet<ActivityRecord> Activities => Set<ActivityRecord>();
    public DbSet<ApplianceUsage> Appliances => Set<ApplianceUsage>();
    public DbSet<EmissionFactor> EmissionFactors => Set<EmissionFactor>();
    public DbSet<EmissionCalculation> EmissionCalculations => Set<EmissionCalculation>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioChange> ScenarioChanges => Set<ScenarioChange>();
    public DbSet<ScenarioResult> ScenarioResults => Set<ScenarioResult>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    async Task IUnitOfWork.SaveChangesAsync() => await base.SaveChangesAsync();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<decimal>().HavePrecision(18, 6);
        b.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<UserProfile>(e =>
        {
            e.ToTable("user_profiles");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasMany(x => x.Vehicles).WithOne().HasForeignKey(v => v.UserProfileId).OnDelete(DeleteBehavior.Cascade);
        });
        m.Entity<Vehicle>(e => { e.ToTable("vehicles"); e.Property(x => x.Name).HasMaxLength(100).IsRequired(); });
        m.Entity<VehicleMonthlyInput>(e =>
        {
            e.ToTable("vehicle_monthly_inputs");
            e.Property(x => x.Month).HasColumnType("date");
            e.HasIndex(x => new { x.VehicleId, x.Month }).IsUnique();
            e.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Cascade);
        });

        m.Entity<ActivityRecord>(e =>
        {
            e.ToTable("activity_records");
            e.HasDiscriminator<string>("ActivityType")
                .HasValue<MobilityActivity>("Mobility").HasValue<FuelPurchaseActivity>("FuelPurchase").HasValue<ElectricityUsage>("Electricity");
            e.Ignore(x => x.Category).Ignore(x => x.ReferenceDate).Ignore(x => x.FactorSubcategory);
            e.Property(x => x.Unit);
            e.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.UserProfileId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Calculation).WithOne().HasForeignKey<EmissionCalculation>(c => c.ActivityRecordId).OnDelete(DeleteBehavior.Cascade);
        });
        m.Entity<MobilityActivity>(e =>
        {
            e.Property(x => x.ActivityDate).HasColumnType("date");
            e.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("ck_mobility_positive", "\"ActivityType\" <> 'Mobility' OR (\"DistanceKm\" > 0 AND \"Frequency\" >= 1)"));
        });
        m.Entity<FuelPurchaseActivity>(e =>
        {
            e.Property(x => x.PurchaseDate).HasColumnType("date");
            e.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<ElectricityUsage>(e =>
        {
            e.Property(x => x.PeriodStart).HasColumnType("date");
            e.Property(x => x.PeriodEnd).HasColumnType("date");
            e.HasMany(x => x.Appliances).WithOne().HasForeignKey(a => a.ElectricityUsageId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserProfileId, x.PeriodStart }).IsUnique(); // satu listrik per profil per bulan
        });
        m.Entity<ApplianceUsage>(e => { e.ToTable("appliance_usages"); e.Property(x => x.Name).HasMaxLength(100).IsRequired(); e.Property(x => x.HoursPerDay).HasPrecision(5, 2); });

        m.Entity<EmissionFactor>(e =>
        {
            e.ToTable("emission_factors");
            e.Property(x => x.Subcategory).HasMaxLength(50).IsRequired();
            e.Property(x => x.Source).HasMaxLength(300).IsRequired();
            e.Property(x => x.Version).HasMaxLength(50).IsRequired();
            e.Property(x => x.ValidFrom).HasColumnType("date");
            e.Property(x => x.ValidUntil).HasColumnType("date");
            e.ToTable(t => t.HasCheckConstraint("ck_factor_nonneg", "\"Value\" >= 0"));
        });
        m.Entity<EmissionCalculation>(e =>
        {
            e.ToTable("emission_calculations");
            e.HasIndex(x => x.ActivityRecordId).IsUnique(); // satu hasil per aktivitas
            e.HasOne(x => x.EmissionFactor).WithMany().HasForeignKey(x => x.EmissionFactorId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<Scenario>(e =>
        {
            e.ToTable("scenarios");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.BaselineStart).HasColumnType("date");
            e.Property(x => x.BaselineEnd).HasColumnType("date");
            e.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.UserProfileId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Changes).WithOne().HasForeignKey(c => c.ScenarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Result).WithOne().HasForeignKey<ScenarioResult>(r => r.ScenarioId).OnDelete(DeleteBehavior.Cascade);
        });
        // Target perubahan memakai id aktivitas apa adanya (snapshot), tanpa FK, agar riwayat skenario tetap utuh bila aktivitas dihapus.
        m.Entity<ScenarioChange>(e => e.ToTable("scenario_changes"));
        m.Entity<ScenarioResult>(e =>
        {
            e.ToTable("scenario_results");
            e.HasMany(x => x.Recommendations).WithOne().HasForeignKey(r => r.ScenarioResultId).OnDelete(DeleteBehavior.Cascade);
        });
        m.Entity<Recommendation>(e =>
        {
            e.ToTable("recommendations");
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.RecommendedAction).HasMaxLength(1000);
        });
    }
}
