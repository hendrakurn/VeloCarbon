export interface Vehicle {
  id: string;
  name: string;
  vehicleType: 'Motorcycle' | 'Car';
  fuelType: 'Pertalite' | 'Pertamax' | 'Solar' | 'Electric';
}

export interface EmissionFactor {
  id: string;
  category: 'Mobility' | 'Electricity';
  subcategory: string;
  value: number; // kgCO2e per unit
  unit: string;
  source: string;
  version: string;
  validFrom: string;
}

export interface ActivityRecord {
  id: string;
  date: string;
  category: 'Mobility' | 'Electricity';
  method: 'Distance' | 'FuelPurchase' | 'MeterKWh' | 'ApplianceEstimate';
  vehicleName?: string;
  details: string;
  activityValue: number;
  unit: string;
  emissionFactorValue: number;
  resultKgCO2e: number;
}

export interface ApplianceItem {
  id: string;
  name: string;
  powerWatt: number;
  quantity: number;
  hoursPerDay: number;
  daysUsed: number;
}

export const initialProfile = {
  id: 'usr-1',
  name: 'Hendra Kurnia Maliqi',
  email: 'hendra.maliqi@mail.ugm.ac.id',
  nim: '24/542344/TK/60216',
  unitPreference: 'Metric (km, L, kWh)',
};

export const initialVehicles: Vehicle[] = [
  { id: 'veh-1', name: 'Honda Vario 125', vehicleType: 'Motorcycle', fuelType: 'Pertalite' },
  { id: 'veh-2', name: 'Toyota Avanza 1.3', vehicleType: 'Car', fuelType: 'Pertamax' },
];

export const initialEmissionFactors: EmissionFactor[] = [
  {
    id: 'ef-1',
    category: 'Mobility',
    subcategory: 'Pertalite (RON 90)',
    value: 2.19,
    unit: 'kgCO2e/liter',
    source: 'KLHK RI & IPCC Guidelines',
    version: '2024.1',
    validFrom: '2024-01-01',
  },
  {
    id: 'ef-2',
    category: 'Mobility',
    subcategory: 'Pertamax (RON 92)',
    value: 2.24,
    unit: 'kgCO2e/liter',
    source: 'KLHK RI & IPCC Guidelines',
    version: '2024.1',
    validFrom: '2024-01-01',
  },
  {
    id: 'ef-3',
    category: 'Mobility',
    subcategory: 'Motorcycle Average (Distance)',
    value: 0.052,
    unit: 'kgCO2e/km',
    source: 'Kemenhub / IPCC Tier 1',
    version: '2024.1',
    validFrom: '2024-01-01',
  },
  {
    id: 'ef-4',
    category: 'Mobility',
    subcategory: 'Passenger Car Average (Distance)',
    value: 0.178,
    unit: 'kgCO2e/km',
    source: 'Kemenhub / IPCC Tier 1',
    version: '2024.1',
    validFrom: '2024-01-01',
  },
  {
    id: 'ef-5',
    category: 'Electricity',
    subcategory: 'Jaringan Listrik Jamali (PLN)',
    value: 0.784,
    unit: 'kgCO2e/kWh',
    source: 'ESDM / Bauran Emisi PLN 2023',
    version: '2023.v2',
    validFrom: '2023-06-01',
  },
];

export const initialActivities: ActivityRecord[] = [
  {
    id: 'act-1',
    date: '2026-09-02',
    category: 'Mobility',
    method: 'FuelPurchase',
    vehicleName: 'Honda Vario 125',
    details: 'Pembelian 4.50 L Pertalite',
    activityValue: 4.5,
    unit: 'liter',
    emissionFactorValue: 2.19,
    resultKgCO2e: 9.855,
  },
  {
    id: 'act-2',
    date: '2026-09-10',
    category: 'Mobility',
    method: 'FuelPurchase',
    vehicleName: 'Honda Vario 125',
    details: 'Pembelian 4.00 L Pertalite',
    activityValue: 4.0,
    unit: 'liter',
    emissionFactorValue: 2.19,
    resultKgCO2e: 8.76,
  },
  {
    id: 'act-3',
    date: '2026-09-18',
    category: 'Mobility',
    method: 'FuelPurchase',
    vehicleName: 'Honda Vario 125',
    details: 'Pembelian 4.20 L Pertalite',
    activityValue: 4.2,
    unit: 'liter',
    emissionFactorValue: 2.19,
    resultKgCO2e: 9.198,
  },
  {
    id: 'act-4',
    date: '2026-09-30',
    category: 'Electricity',
    method: 'ApplianceEstimate',
    details: 'Estimasi Kamar Kos: AC (1/2 PK), Laptop, Lampu LED, Kipas',
    activityValue: 72.8,
    unit: 'kWh',
    emissionFactorValue: 0.784,
    resultKgCO2e: 57.075,
  },
];

export const initialMonthlyTrend = [
  { month: 'Apr 2026', mobility: 28.5, electricity: 54.0, total: 82.5 },
  { month: 'Mei 2026', mobility: 32.1, electricity: 59.2, total: 91.3 },
  { month: 'Jun 2026', mobility: 26.4, electricity: 52.1, total: 78.5 },
  { month: 'Jul 2026', mobility: 34.0, electricity: 61.5, total: 95.5 },
  { month: 'Agu 2026', mobility: 30.2, electricity: 58.0, total: 88.2 },
  { month: 'Sep 2026', mobility: 27.813, electricity: 57.075, total: 84.888 },
];
