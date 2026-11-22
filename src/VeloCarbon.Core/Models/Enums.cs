namespace VeloCarbon.Core.Models;

public enum ActivityUnit { Km, Liter, KWh }
public enum EmissionCategory { Mobility, Electricity }
public enum TransportMode { Walk, Bicycle, Bus, Train, Motorcycle, Car }
public enum FuelType { Petrol, Diesel }
public enum MobilityInputMethod { Distance, FuelPurchase }
public enum ElectricityInputMethod { MeterKWh, ApplianceEstimate }
public enum ChangeType { ReduceDistance, ReduceLiters, ReduceKWh, ReduceApplianceHours }
public enum UnitPreference { Metric }
