using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public record DistanceInput(Guid ProfileId, Guid? VehicleId, DateTime Date, TransportMode Mode, decimal DistanceKm, int Frequency);
public record FuelPurchaseInput(Guid ProfileId, Guid VehicleId, DateTime Date, FuelType FuelType, decimal Liters, decimal? TotalCost);
public record MeterElectricityInput(Guid ProfileId, DateTime Month, decimal UsageKWh);
public record ApplianceInput(string Name, decimal PowerWatt, int Quantity, decimal HoursPerDay, int DaysUsed);
public record ApplianceElectricityInput(Guid ProfileId, DateTime Month, IReadOnlyList<ApplianceInput> Appliances);

public record CategoryShare(string Label, EmissionCategory Category, decimal KgCO2e, decimal SharePercent);
public record DashboardSummary(DateTime Month, decimal TotalKgCO2e, IReadOnlyList<CategoryShare> Categories, string? TopSource);
public record TrendPoint(DateTime Month, decimal TotalKgCO2e);

public record ScenarioChangeInput(ChangeType ChangeType, Guid TargetActivityRecordId, Guid? TargetApplianceUsageId, decimal ReductionPercent);
public record ScenarioRequest(Guid ProfileId, string Name, DateTime Month, IReadOnlyList<ScenarioChangeInput> Changes);
