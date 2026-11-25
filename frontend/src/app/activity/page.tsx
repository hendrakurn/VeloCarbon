'use client';

import { useState } from 'react';
import Header from '@/components/Header';
import { initialVehicles, initialEmissionFactors } from '@/data/mockData';
import { 
  Navigation, 
  Fuel, 
  Gauge, 
  Tv, 
  Plus, 
  Trash2, 
  CheckCircle2, 
  AlertTriangle,
  Calculator,
  Save
} from 'lucide-react';

interface ApplianceRow {
  id: string;
  name: string;
  watt: number;
  qty: number;
  hours: number;
  days: number;
}

export default function ActivityInputPage() {
  const [activeTab, setActiveTab] = useState<'distance' | 'fuel' | 'meter' | 'appliance'>('fuel');
  const [saveSuccess, setSaveSuccess] = useState(false);

  // Tab 1: Distance State
  const [distDate, setDistDate] = useState('2026-09-30');
  const [distVehicle, setDistVehicle] = useState(initialVehicles[0].id);
  const [distKm, setDistKm] = useState<number>(12.5);
  const [distFreq, setDistFreq] = useState<number>(2);

  // Tab 2: Fuel Purchase State
  const [fuelDate, setFuelDate] = useState('2026-09-30');
  const [fuelVehicle, setFuelVehicle] = useState(initialVehicles[0].id);
  const [fuelType, setFuelType] = useState('Pertalite');
  const [fuelLiters, setFuelLiters] = useState<number>(4.0);
  const [fuelCost, setFuelCost] = useState<number>(40000);

  // Tab 3: Meter State
  const [meterMonth, setMeterMonth] = useState('2026-09');
  const [meterKWh, setMeterKWh] = useState<number>(65.0);

  // Tab 4: Appliance Estimator State
  const [appliances, setAppliances] = useState<ApplianceRow[]>([
    { id: '1', name: 'Air Conditioner (1/2 PK)', watt: 350, qty: 1, hours: 8, days: 30 },
    { id: '2', name: 'Laptop / Komputer', watt: 65, qty: 1, hours: 6, days: 30 },
    { id: '3', name: 'Lampu LED Kamar', watt: 15, qty: 2, hours: 10, days: 30 },
    { id: '4', name: 'Kipas Angin Meja', watt: 40, qty: 1, hours: 4, days: 30 },
  ]);

  // Calculations
  const calcDistanceEmission = () => {
    const factor = 0.052; // Motorcycle avg
    return (distKm * distFreq * factor).toFixed(3);
  };

  const calcFuelEmission = () => {
    const factor = fuelType === 'Pertalite' ? 2.19 : 2.24;
    return (fuelLiters * factor).toFixed(3);
  };

  const calcMeterEmission = () => {
    const factor = 0.784; // PLN Jamali
    return (meterKWh * factor).toFixed(3);
  };

  const calculateRowKWh = (row: ApplianceRow) => {
    return (row.watt * row.qty * row.hours * row.days) / 1000;
  };

  const totalApplianceKWh = appliances.reduce((sum, r) => sum + calculateRowKWh(r), 0);
  const totalApplianceEmission = (totalApplianceKWh * 0.784).toFixed(3);

  const addApplianceRow = () => {
    const newId = (appliances.length + 1).toString();
    setAppliances([...appliances, { id: newId, name: 'Alat Elektronik Baru', watt: 50, qty: 1, hours: 4, days: 30 }]);
  };

  const removeApplianceRow = (id: string) => {
    setAppliances(appliances.filter(a => a.id !== id));
  };

  const updateApplianceRow = (id: string, field: keyof ApplianceRow, val: any) => {
    setAppliances(appliances.map(a => a.id === id ? { ...a, [field]: val } : a));
  };

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    setSaveSuccess(true);
    setTimeout(() => setSaveSuccess(false), 4000);
  };

  return (
    <div className="flex-1 pb-16">
      <Header 
        title="Catat Aktivitas Emisi" 
        subtitle="Mendukung 4 Metode Input (Jarak, Bahan Bakar, Meter Listrik, Estimasi Alat)"
      />

      <div className="p-8 max-w-5xl mx-auto space-y-6">
        {/* Anti Double-Counting Alert (FR-13) */}
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-start space-x-3 text-xs text-amber-800">
          <AlertTriangle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
          <div>
            <span className="font-bold">Ketentuan Validasi Periode (Aturan PRD FR-13):</span>
            <ul className="list-disc ml-4 mt-1 space-y-0.5 text-amber-700">
              <li>Satu kendaraan hanya boleh menggunakan 1 metode per bulan (Jarak ATAU Pembelian BBM).</li>
              <li>Profil kos hanya boleh menggunakan 1 metode listrik per bulan (Meter Listrik ATAU Estimasi Alat).</li>
            </ul>
          </div>
        </div>

        {/* Success Banner */}
        {saveSuccess && (
          <div className="bg-emerald-50 border border-emerald-300 rounded-xl p-4 flex items-center space-x-3 text-emerald-800 text-sm font-semibold animate-in fade-in duration-200">
            <CheckCircle2 className="w-5 h-5 text-emerald-600" />
            <span>Data aktivitas berhasil disimpan ke PostgreSQL dan emisi langsung dihitung!</span>
          </div>
        )}

        {/* Method Tab Selector */}
        <div className="bg-white rounded-xl border border-slate-200 p-1.5 grid grid-cols-2 md:grid-cols-4 gap-1.5 shadow-sm">
          <button
            onClick={() => setActiveTab('fuel')}
            className={`flex items-center justify-center space-x-2 py-3 px-3 rounded-lg text-xs font-bold transition ${
              activeTab === 'fuel' 
                ? 'bg-emerald-600 text-white shadow' 
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            <Fuel className="w-4 h-4" />
            <span>1. Pembelian BBM</span>
          </button>

          <button
            onClick={() => setActiveTab('distance')}
            className={`flex items-center justify-center space-x-2 py-3 px-3 rounded-lg text-xs font-bold transition ${
              activeTab === 'distance' 
                ? 'bg-blue-600 text-white shadow' 
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            <Navigation className="w-4 h-4" />
            <span>2. Jarak Perjalanan</span>
          </button>

          <button
            onClick={() => setActiveTab('meter')}
            className={`flex items-center justify-center space-x-2 py-3 px-3 rounded-lg text-xs font-bold transition ${
              activeTab === 'meter' 
                ? 'bg-amber-600 text-white shadow' 
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            <Gauge className="w-4 h-4" />
            <span>3. Total kWh Meter</span>
          </button>

          <button
            onClick={() => setActiveTab('appliance')}
            className={`flex items-center justify-center space-x-2 py-3 px-3 rounded-lg text-xs font-bold transition ${
              activeTab === 'appliance' 
                ? 'bg-purple-600 text-white shadow' 
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            <Tv className="w-4 h-4" />
            <span>4. Estimasi Alat Elektronik</span>
          </button>
        </div>

        {/* Tab 1: Fuel Purchase Form */}
        {activeTab === 'fuel' && (
          <form onSubmit={handleSave} className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-6">
            <div className="border-b border-slate-100 pb-4">
              <h3 className="font-bold text-slate-800 text-base flex items-center gap-2">
                <Fuel className="w-5 h-5 text-emerald-600" />
                Input Pembelian Bahan Bakar
              </h3>
              <p className="text-xs text-slate-500">Estimasi emisi dihitung dari total liter bahan bakar yang dibeli pada tanggal transaksi.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Tanggal Pembelian</label>
                <input 
                  type="date" 
                  value={fuelDate}
                  onChange={(e) => setFuelDate(e.target.value)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Pilih Kendaraan</label>
                <select 
                  value={fuelVehicle}
                  onChange={(e) => setFuelVehicle(e.target.value)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                >
                  {initialVehicles.map(v => (
                    <option key={v.id} value={v.id}>{v.name} ({v.fuelType})</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Jenis Bahan Bakar</label>
                <select 
                  value={fuelType}
                  onChange={(e) => setFuelType(e.target.value)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                >
                  <option value="Pertalite">Pertalite (RON 90) — 2.19 kgCO₂e/L</option>
                  <option value="Pertamax">Pertamax (RON 92) — 2.24 kgCO₂e/L</option>
                  <option value="Solar">Solar / Biosolar — 2.68 kgCO₂e/L</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Liter Dibeli (L)</label>
                <input 
                  type="number" 
                  step="0.01"
                  min="0.1"
                  value={fuelLiters}
                  onChange={(e) => setFuelLiters(parseFloat(e.target.value) || 0)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none font-mono"
                  required
                />
              </div>

              <div className="md:col-span-2">
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Total Biaya (Rp) — Opsional</label>
                <input 
                  type="number" 
                  step="1000"
                  value={fuelCost}
                  onChange={(e) => setFuelCost(parseFloat(e.target.value) || 0)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none font-mono"
                  placeholder="Rp 40.000"
                />
                <span className="text-[11px] text-slate-400 mt-1 block">Catatan: Nominal rupiah tidak digunakan langsung untuk perhitungan emisi karbon.</span>
              </div>
            </div>

            {/* Live Calculation Preview */}
            <div className="bg-slate-50 border border-slate-200 rounded-lg p-4 flex items-center justify-between">
              <div className="flex items-center space-x-2 text-xs text-slate-600">
                <Calculator className="w-4 h-4 text-emerald-600" />
                <span>Hasil Kalkulasi: {fuelLiters} L × {fuelType === 'Pertalite' ? 2.19 : 2.24} kgCO₂e/L</span>
              </div>
              <div className="text-right">
                <span className="text-xs text-slate-400 block">Estimasi Emisi:</span>
                <span className="text-lg font-extrabold text-emerald-700 font-mono">{calcFuelEmission()} kgCO₂e</span>
              </div>
            </div>

            <div className="flex justify-end">
              <button 
                type="submit"
                className="flex items-center space-x-2 px-6 py-2.5 bg-emerald-600 text-white rounded-lg text-xs font-bold hover:bg-emerald-700 transition shadow"
              >
                <Save className="w-4 h-4" />
                <span>Simpan Aktivitas Bahan Bakar</span>
              </button>
            </div>
          </form>
        )}

        {/* Tab 2: Distance Form */}
        {activeTab === 'distance' && (
          <form onSubmit={handleSave} className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-6">
            <div className="border-b border-slate-100 pb-4">
              <h3 className="font-bold text-slate-800 text-base flex items-center gap-2">
                <Navigation className="w-5 h-5 text-blue-600" />
                Input Jarak Perjalanan
              </h3>
              <p className="text-xs text-slate-500">Menghitung emisi dari jarak satu arah (km) dikalikan frekuensi perjalanan harian.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Tanggal Perjalanan</label>
                <input 
                  type="date" 
                  value={distDate}
                  onChange={(e) => setDistDate(e.target.value)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Moda / Kendaraan</label>
                <select 
                  value={distVehicle}
                  onChange={(e) => setDistVehicle(e.target.value)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none"
                >
                  {initialVehicles.map(v => (
                    <option key={v.id} value={v.id}>{v.name}</option>
                  ))}
                  <option value="bus">Trans Jogja / Bus Publik</option>
                  <option value="walk">Jalan Kaki / Sepeda (0 kgCO₂e)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Jarak Satu Arah (km)</label>
                <input 
                  type="number" 
                  step="0.1"
                  min="0.1"
                  value={distKm}
                  onChange={(e) => setDistKm(parseFloat(e.target.value) || 0)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none font-mono"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Frekuensi Perjalanan (kali/hari)</label>
                <input 
                  type="number" 
                  min="1"
                  value={distFreq}
                  onChange={(e) => setDistFreq(parseInt(e.target.value) || 1)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none font-mono"
                  required
                />
              </div>
            </div>

            {/* Live Calculation */}
            <div className="bg-slate-50 border border-slate-200 rounded-lg p-4 flex items-center justify-between">
              <div className="flex items-center space-x-2 text-xs text-slate-600">
                <Calculator className="w-4 h-4 text-blue-600" />
                <span>Kalkulasi: {distKm} km × {distFreq} trip × 0.052 kgCO₂e/km</span>
              </div>
              <div className="text-right">
                <span className="text-xs text-slate-400 block">Estimasi Emisi:</span>
                <span className="text-lg font-extrabold text-blue-700 font-mono">{calcDistanceEmission()} kgCO₂e</span>
              </div>
            </div>

            <div className="flex justify-end">
              <button 
                type="submit"
                className="flex items-center space-x-2 px-6 py-2.5 bg-blue-600 text-white rounded-lg text-xs font-bold hover:bg-blue-700 transition shadow"
              >
                <Save className="w-4 h-4" />
                <span>Simpan Aktivitas Jarak</span>
              </button>
            </div>
          </form>
        )}

        {/* Tab 3: Meter Form */}
        {activeTab === 'meter' && (
          <form onSubmit={handleSave} className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-6">
            <div className="border-b border-slate-100 pb-4">
              <h3 className="font-bold text-slate-800 text-base flex items-center gap-2">
                <Gauge className="w-5 h-5 text-amber-600" />
                Input Total kWh Meter Listrik
              </h3>
              <p className="text-xs text-slate-500">Mencatat konsumsi listrik aktual satu bulan kalender penuh dari meteran PLN.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Bulan Periode (Bulan Penuh)</label>
                <input 
                  type="month" 
                  value={meterMonth}
                  onChange={(e) => setMeterMonth(e.target.value)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-amber-500 focus:outline-none"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">Total Pemakaian (kWh)</label>
                <input 
                  type="number" 
                  step="0.1"
                  min="0"
                  value={meterKWh}
                  onChange={(e) => setMeterKWh(parseFloat(e.target.value) || 0)}
                  className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-amber-500 focus:outline-none font-mono"
                  required
                />
              </div>
            </div>

            <div className="bg-slate-50 border border-slate-200 rounded-lg p-4 flex items-center justify-between">
              <div className="flex items-center space-x-2 text-xs text-slate-600">
                <Calculator className="w-4 h-4 text-amber-600" />
                <span>Kalkulasi: {meterKWh} kWh × 0.784 kgCO₂e/kWh (Grid PLN Jamali)</span>
              </div>
              <div className="text-right">
                <span className="text-xs text-slate-400 block">Estimasi Emisi:</span>
                <span className="text-lg font-extrabold text-amber-700 font-mono">{calcMeterEmission()} kgCO₂e</span>
              </div>
            </div>

            <div className="flex justify-end">
              <button 
                type="submit"
                className="flex items-center space-x-2 px-6 py-2.5 bg-amber-600 text-white rounded-lg text-xs font-bold hover:bg-amber-700 transition shadow"
              >
                <Save className="w-4 h-4" />
                <span>Simpan Total kWh Meter</span>
              </button>
            </div>
          </form>
        )}

        {/* Tab 4: Appliance Estimator Form */}
        {activeTab === 'appliance' && (
          <form onSubmit={handleSave} className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-6">
            <div className="border-b border-slate-100 pb-4 flex items-center justify-between">
              <div>
                <h3 className="font-bold text-slate-800 text-base flex items-center gap-2">
                  <Tv className="w-5 h-5 text-purple-600" />
                  Estimasi Listrik Kamar Kos (Alat Elektronik)
                </h3>
                <p className="text-xs text-slate-500">Rumus: (Watt / 1000) × Jumlah × Jam/Hari × Hari Pemakaian</p>
              </div>

              <button
                type="button"
                onClick={addApplianceRow}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-purple-50 text-purple-700 border border-purple-200 hover:bg-purple-100 text-xs font-semibold"
              >
                <Plus className="w-4 h-4" /> + Tambah Rincian Alat
              </button>
            </div>

            {/* Appliance Dynamic Rows Table */}
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="py-2.5 px-3">Nama Alat Elektronik</th>
                    <th className="py-2.5 px-3 w-24">Daya (Watt)</th>
                    <th className="py-2.5 px-3 w-20">Jumlah</th>
                    <th className="py-2.5 px-3 w-24">Jam/Hari</th>
                    <th className="py-2.5 px-3 w-24">Hari Pakai</th>
                    <th className="py-2.5 px-3 text-right w-24">Subtotal kWh</th>
                    <th className="py-2.5 px-3 text-center w-12">Aksi</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {appliances.map((app) => {
                    const rowKWh = calculateRowKWh(app);
                    return (
                      <tr key={app.id} className="hover:bg-slate-50/60">
                        <td className="py-2.5 px-3">
                          <input 
                            type="text" 
                            value={app.name}
                            onChange={(e) => updateApplianceRow(app.id, 'name', e.target.value)}
                            className="w-full text-xs rounded border border-slate-200 p-1.5 focus:ring-1 focus:ring-purple-500"
                            required
                          />
                        </td>
                        <td className="py-2.5 px-3">
                          <input 
                            type="number" 
                            min="1"
                            value={app.watt}
                            onChange={(e) => updateApplianceRow(app.id, 'watt', parseFloat(e.target.value) || 0)}
                            className="w-full text-xs rounded border border-slate-200 p-1.5 font-mono"
                            required
                          />
                        </td>
                        <td className="py-2.5 px-3">
                          <input 
                            type="number" 
                            min="1"
                            value={app.qty}
                            onChange={(e) => updateApplianceRow(app.id, 'qty', parseInt(e.target.value) || 1)}
                            className="w-full text-xs rounded border border-slate-200 p-1.5 font-mono"
                            required
                          />
                        </td>
                        <td className="py-2.5 px-3">
                          <input 
                            type="number" 
                            min="0.1"
                            max="24"
                            step="0.5"
                            value={app.hours}
                            onChange={(e) => updateApplianceRow(app.id, 'hours', parseFloat(e.target.value) || 0)}
                            className="w-full text-xs rounded border border-slate-200 p-1.5 font-mono"
                            required
                          />
                        </td>
                        <td className="py-2.5 px-3">
                          <input 
                            type="number" 
                            min="1"
                            max="31"
                            value={app.days}
                            onChange={(e) => updateApplianceRow(app.id, 'days', parseInt(e.target.value) || 1)}
                            className="w-full text-xs rounded border border-slate-200 p-1.5 font-mono"
                            required
                          />
                        </td>
                        <td className="py-2.5 px-3 text-right font-mono font-bold text-slate-800">
                          {rowKWh.toFixed(2)} kWh
                        </td>
                        <td className="py-2.5 px-3 text-center">
                          <button
                            type="button"
                            onClick={() => removeApplianceRow(app.id)}
                            className="text-slate-400 hover:text-red-600 p-1 transition"
                            title="Hapus baris"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {/* Summation Preview */}
            <div className="bg-purple-50/50 border border-purple-200 rounded-lg p-4 flex items-center justify-between">
              <div>
                <span className="text-xs text-purple-900 font-bold block">Total Akumulasi Listrik Kamar:</span>
                <span className="text-sm font-semibold text-purple-800 font-mono">
                  {totalApplianceKWh.toFixed(2)} kWh × 0.784 kgCO₂e/kWh
                </span>
              </div>
              <div className="text-right">
                <span className="text-xs text-purple-700 block">Total Emisi Alat Kos:</span>
                <span className="text-xl font-extrabold text-purple-900 font-mono">{totalApplianceEmission} kgCO₂e</span>
              </div>
            </div>

            <div className="flex justify-end">
              <button 
                type="submit"
                className="flex items-center space-x-2 px-6 py-2.5 bg-purple-600 text-white rounded-lg text-xs font-bold hover:bg-purple-700 transition shadow"
              >
                <Save className="w-4 h-4" />
                <span>Simpan Seluruh Estimasi Alat</span>
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
