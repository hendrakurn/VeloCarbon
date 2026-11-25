namespace VeloCarbon.Core.Models;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public UnitPreference UnitPreference { get; set; } = UnitPreference.Metric;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Vehicle> Vehicles { get; set; } = new();
}

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = "";
    public TransportMode VehicleType { get; set; }
    public FuelType FuelType { get; set; }
}

/// <summary>Satu metode mobilitas (Distance/FuelPurchase) per kendaraan per bulan.</summary>
public class VehicleMonthlyInput
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VehicleId { get; set; }
    /// <summary>Tanggal pertama bulan.</summary>
    public DateTime Month { get; set; }
    public MobilityInputMethod Method { get; set; }
}

public abstract class ActivityRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public ActivityUnit Unit { get; protected set; }
    public abstract EmissionCategory Category { get; }
    /// <summary>Tanggal acuan (tanggal aktivitas / awal periode) untuk pemilihan faktor dan bulan laporan.</summary>
    public abstract DateTime ReferenceDate { get; }
    /// <summary>Subkategori faktor emisi yang dibutuhkan (moda, jenis BBM, atau "Grid").</summary>
    public abstract string FactorSubcategory { get; }
    public abstract decimal GetActivityValue();
    public EmissionCalculation? Calculation { get; set; }
}

public class MobilityActivity : ActivityRecord
{
    public MobilityActivity() { Unit = ActivityUnit.Km; }
    public Guid? VehicleId { get; set; }
    public DateTime ActivityDate { get; set; }
    public TransportMode TransportMode { get; set; }
    public decimal DistanceKm { get; set; }
    public int Frequency { get; set; } = 1;
    public override EmissionCategory Category => EmissionCategory.Mobility;
    public override DateTime ReferenceDate => ActivityDate;
    public override string FactorSubcategory => TransportMode.ToString();
    public override decimal GetActivityValue() => DistanceKm * Frequency;
}

public class FuelPurchaseActivity : ActivityRecord
{
    public FuelPurchaseActivity() { Unit = ActivityUnit.Liter; }
    public Guid VehicleId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public FuelType FuelType { get; set; }
    public decimal LitersPurchased { get; set; }
    public decimal? TotalCost { get; set; }
    public override EmissionCategory Category => EmissionCategory.Mobility;
    public override DateTime ReferenceDate => PurchaseDate;
    public override string FactorSubcategory => FuelType.ToString();
    public override decimal GetActivityValue() => LitersPurchased;
}

public class ElectricityUsage : ActivityRecord
{
    public ElectricityUsage() { Unit = ActivityUnit.KWh; }
    public ElectricityInputMethod InputMethod { get; set; }
    /// <summary>Diisi langsung (meter) atau dihitung dari rincian alat.</summary>
    public decimal UsageKWh { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ApplianceUsage> Appliances { get; set; } = new();
    public override EmissionCategory Category => EmissionCategory.Electricity;
    public override DateTime ReferenceDate => PeriodStart;
    public override string FactorSubcategory => "Grid";
    public override decimal GetActivityValue() => UsageKWh;

    public void RecalculateFromAppliances() => UsageKWh = Appliances.Sum(a => a.EstimateKWh());
}

public class ApplianceUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ElectricityUsageId { get; set; }
    public string Name { get; set; } = "";
    public decimal PowerWatt { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal HoursPerDay { get; set; }
    public int DaysUsed { get; set; }
    public decimal EstimateKWh() => PowerWatt / 1000m * Quantity * HoursPerDay * DaysUsed;
}

public class EmissionFactor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmissionCategory Category { get; set; }
    public string Subcategory { get; set; } = "";
    public decimal Value { get; set; }
    public ActivityUnit Unit { get; set; }
    public string Source { get; set; } = "";
    public string Version { get; set; } = "";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;

    public bool AppliesOn(DateTime date) =>
        IsActive && date.Date >= ValidFrom.Date && (ValidUntil == null || date.Date <= ValidUntil.Value.Date);
}

public class EmissionCalculation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActivityRecordId { get; set; }
    public Guid EmissionFactorId { get; set; }
    public EmissionFactor? EmissionFactor { get; set; }
    public decimal ActivityValue { get; set; }
    public decimal ResultKgCO2e { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class Scenario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = "";
    public DateTime BaselineStart { get; set; }
    public DateTime BaselineEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ScenarioChange> Changes { get; set; } = new();
    public ScenarioResult? Result { get; set; }
}

public class ScenarioChange
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioId { get; set; }
    public ChangeType ChangeType { get; set; }
    public Guid TargetActivityRecordId { get; set; }
    public Guid? TargetApplianceUsageId { get; set; }
    /// <summary>Persentase pengurangan, 0 &lt; nilai &lt;= 100.</summary>
    public decimal ChangeValue { get; set; }
}

public class ScenarioResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioId { get; set; }
    public decimal BaselineKgCO2e { get; set; }
    public decimal ScenarioKgCO2e { get; set; }
    public decimal ReductionKgCO2e { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public List<Recommendation> Recommendations { get; set; } = new();

    public decimal? GetReductionPercentage() =>
        BaselineKgCO2e == 0 ? null : ReductionKgCO2e / BaselineKgCO2e * 100m;
}

public class Recommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScenarioResultId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public EmissionCategory TargetCategory { get; set; }
    public string RecommendedAction { get; set; } = "";
    public decimal EstimatedReductionKgCO2e { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
