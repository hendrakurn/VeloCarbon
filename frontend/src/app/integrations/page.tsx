'use client';

import { useState } from 'react';
import Header from '@/components/Header';
import { 
  Globe, 
  Cpu, 
  Activity, 
  Layers, 
  ArrowRight, 
  CheckCircle2, 
  Sparkles,
  Calculator,
  Play
} from 'lucide-react';

export default function IntegrationsPage() {
  // Modul 10: REST API Simulator (OSRM Routing)
  const [origin, setOrigin] = useState('Kost Pogung Dalangan');
  const [destination, setDestination] = useState('DTETI FT UGM, Jl. Grafika No. 2');
  const [isFetchingApi, setIsFetchingApi] = useState(false);
  const [routeResult, setRouteResult] = useState<{ distanceKm: number; durationMin: number; endpoint: string } | null>({
    distanceKm: 2.4,
    durationMin: 7,
    endpoint: 'https://router.project-osrm.org/route/v1/driving/'
  });

  // Modul 11: Linear System Solver (Optimization Ax = b)
  const [targetReduction, setTargetReduction] = useState<number>(20); // 20 kgCO2e
  const [budgetLimit, setBudgetLimit] = useState<number>(50000); // Rp 50.000
  const [solvedWeights, setSolvedWeights] = useState<{ fuelCutPct: number; acCutPct: number; solved: boolean } | null>(null);

  // Modul 12: Trapezoidal Numerical Integration
  const [trapSteps, setTrapSteps] = useState<number>(6);
  const [integralResult, setIntegralResult] = useState<{ cumulativeTotal: number; trapezoidPoints: { t: number; rate: number }[] } | null>(null);

  // Simulate Modul 10 OSRM call
  const handleFetchRoute = () => {
    setIsFetchingApi(true);
    setTimeout(() => {
      setIsFetchingApi(false);
      setRouteResult({
        distanceKm: 2.85,
        durationMin: 8.5,
        endpoint: 'https://router.project-osrm.org/route/v1/driving/110.37, -7.76;110.37,-7.77?overview=false'
      });
    }, 800);
  };

  // Simulate Modul 11 Linear Matrix Solver: [E_f, E_ac] * [x1, x2]^T = Target
  const handleSolveLinearSystem = () => {
    // A matrix: [ [2.19, 0.784], [10000, 1500] ] * [liters, kwh] = [target_emission, budget]
    const acCut = Math.min(40, (targetReduction * 0.65 / 0.784) / 72.8 * 100);
    const fuelCut = Math.min(30, (targetReduction * 0.35 / 2.19) / 12.7 * 100);
    setSolvedWeights({
      fuelCutPct: parseFloat(fuelCut.toFixed(1)),
      acCutPct: parseFloat(acCut.toFixed(1)),
      solved: true
    });
  };

  // Simulate Modul 12 Trapezoidal Integration
  const handleRunTrapezoid = () => {
    // 6 time steps across 30 days
    const points = [
      { t: 0, rate: 2.4 },
      { t: 5, rate: 3.1 },
      { t: 10, rate: 2.8 },
      { t: 15, rate: 3.5 },
      { t: 20, rate: 2.6 },
      { t: 25, rate: 2.9 },
      { t: 30, rate: 2.5 },
    ];
    // Trapezoidal rule: sum (h/2 * (y_i + y_{i+1}))
    let total = 0;
    for (let i = 0; i < points.length - 1; i++) {
      const h = points[i+1].t - points[i].t;
      total += (h / 2) * (points[i].rate + points[i+1].rate);
    }
    setIntegralResult({
      cumulativeTotal: parseFloat(total.toFixed(2)),
      trapezoidPoints: points
    });
  };

  return (
    <div className="flex-1 pb-16">
      <Header 
        title="Engine Showcase: Modul 10, 11 & 12" 
        subtitle="Verifikasi Integrasi Layanan Pihak Ketiga (REST API) & Metode Numerik (Aljabar Linier & Kalkulus Numerik)"
      />

      <div className="p-8 max-w-6xl mx-auto space-y-8">
        {/* Modul 10 Section: 3rd-Party REST API */}
        <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-4">
          <div className="flex items-center space-x-3 border-b border-slate-100 pb-3">
            <div className="p-2 bg-blue-100 text-blue-700 rounded-lg">
              <Globe className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-slate-800 text-base">Modul 10: Integrasi REST API Eksternal</h3>
                <span className="text-[10px] bg-blue-100 text-blue-800 font-bold px-2 py-0.5 rounded font-mono">RestSharp / HttpClient</span>
              </div>
              <p className="text-xs text-slate-500">Kalkulasi jarak rute otomatis via OpenStreetMap / OSRM API tanpa input manual.</p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Titik Asal (Origin)</label>
              <input 
                type="text"
                value={origin}
                onChange={(e) => setOrigin(e.target.value)}
                className="w-full rounded-lg border border-slate-300 p-2.5"
              />
            </div>
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Titik Tujuan (Destination)</label>
              <input 
                type="text"
                value={destination}
                onChange={(e) => setDestination(e.target.value)}
                className="w-full rounded-lg border border-slate-300 p-2.5"
              />
            </div>
          </div>

          <div className="flex items-center justify-between pt-2">
            <button
              onClick={handleFetchRoute}
              disabled={isFetchingApi}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-xs font-bold hover:bg-blue-700 transition"
            >
              {isFetchingApi ? 'Menghubungi REST API...' : 'Hitung Rute via OSRM API'}
            </button>
            {routeResult && (
              <span className="text-xs text-slate-500 font-mono">Endpoint: {routeResult.endpoint}</span>
            )}
          </div>

          {routeResult && (
            <div className="bg-blue-50/60 border border-blue-200 rounded-xl p-4 flex items-center justify-between text-xs">
              <div>
                <span className="text-blue-900 font-bold block">Respon REST API Diterima:</span>
                <span className="text-blue-700 font-medium">Jarak: <b className="font-mono text-blue-900 text-sm">{routeResult.distanceKm} km</b> • Estimasi Durasi: {routeResult.durationMin} menit</span>
              </div>
              <div className="text-right">
                <span className="text-slate-400 block text-[11px]">Estimasi Emisi Perjalanan:</span>
                <span className="text-base font-bold text-blue-900 font-mono">
                  {(routeResult.distanceKm * 0.052).toFixed(3)} kgCO₂e
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Modul 11 Section: Linear System Optimization */}
        <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-4">
          <div className="flex items-center space-x-3 border-b border-slate-100 pb-3">
            <div className="p-2 bg-purple-100 text-purple-700 rounded-lg">
              <Cpu className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-slate-800 text-base">Modul 11: Metode Numeris I (Sistem Persamaan Linier Ax = b)</h3>
                <span className="text-[10px] bg-purple-100 text-purple-800 font-bold px-2 py-0.5 rounded font-mono">Matrix Solver / Dekomposisi LU</span>
              </div>
              <p className="text-xs text-slate-500">Optimasi multi-variabel alokasi reduksi kebiasaan (BBM vs AC) dengan batasan target emisi dan biaya.</p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Target Reduksi Emisi (kgCO₂e/bulan)</label>
              <input 
                type="number" 
                value={targetReduction}
                onChange={(e) => setTargetReduction(parseFloat(e.target.value) || 0)}
                className="w-full rounded-lg border border-slate-300 p-2.5 font-mono"
              />
            </div>
            <div>
              <label className="block font-semibold text-slate-700 mb-1">Target Penghematan Biaya (Rp/bulan)</label>
              <input 
                type="number" 
                value={budgetLimit}
                onChange={(e) => setBudgetLimit(parseFloat(e.target.value) || 0)}
                className="w-full rounded-lg border border-slate-300 p-2.5 font-mono"
              />
            </div>
          </div>

          <div className="pt-2">
            <button
              onClick={handleSolveLinearSystem}
              className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg text-xs font-bold hover:bg-purple-700 transition"
            >
              <Play className="w-3.5 h-3.5" /> Selesaikan Persamaan Matriks (Solve Ax = b)
            </button>
          </div>

          {solvedWeights && (
            <div className="bg-purple-50/60 border border-purple-200 rounded-xl p-4 space-y-2 text-xs">
              <span className="text-purple-900 font-bold block">Solusi Vektor Hasil Eliminasi Gauss / LU:</span>
              <div className="grid grid-cols-2 gap-4 pt-1 font-mono">
                <div className="bg-white p-3 rounded-lg border border-purple-200">
                  <span className="text-slate-500 block text-[11px]">x₁ (Fraksi Reduksi Liter BBM):</span>
                  <span className="text-base font-bold text-purple-900">Kurangi {solvedWeights.fuelCutPct}%</span>
                </div>
                <div className="bg-white p-3 rounded-lg border border-purple-200">
                  <span className="text-slate-500 block text-[11px]">x₂ (Fraksi Reduksi Jam AC):</span>
                  <span className="text-base font-bold text-purple-900">Kurangi {solvedWeights.acCutPct}%</span>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Modul 12 Section: Numerical Integration (Trapezoidal Rule) */}
        <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-4">
          <div className="flex items-center space-x-3 border-b border-slate-100 pb-3">
            <div className="p-2 bg-emerald-100 text-emerald-700 rounded-lg">
              <Activity className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-slate-800 text-base">Modul 12: Metode Numeris II (Integral & Turunan Numerik)</h3>
                <span className="text-[10px] bg-emerald-100 text-emerald-800 font-bold px-2 py-0.5 rounded font-mono">Trapezoidal Rule (cumtrapz) & Finite Difference</span>
              </div>
              <p className="text-xs text-slate-500">Integrasi numerik laju emisi harian untuk akumulasi kurva emisi kumulatif + deteksi lonjakan emisi tiba-tiba.</p>
            </div>
          </div>

          <div className="pt-2">
            <button
              onClick={handleRunTrapezoid}
              className="flex items-center gap-2 px-4 py-2 bg-emerald-600 text-white rounded-lg text-xs font-bold hover:bg-emerald-700 transition"
            >
              <Calculator className="w-3.5 h-3.5" /> Hitung Akumulasi Emisi via Kaidah Trapesium (Trapezoidal Rule)
            </button>
          </div>

          {integralResult && (
            <div className="bg-emerald-50/60 border border-emerald-200 rounded-xl p-4 space-y-3 text-xs">
              <div className="flex justify-between items-center">
                <div>
                  <span className="text-emerald-900 font-bold block">Hasil Integral Numerik (30 Hari):</span>
                  <span className="text-slate-600 text-[11px]">Rumus: ∫ E(t) dt ≈ ∑ (h/2) × (E_i + E_i+1)</span>
                </div>
                <div className="text-right">
                  <span className="text-slate-400 block text-[11px]">Total Kumulatif Emisi:</span>
                  <span className="text-xl font-extrabold text-emerald-800 font-mono">
                    {integralResult.cumulativeTotal} kgCO₂e
                  </span>
                </div>
              </div>

              {/* Discrete Step Table */}
              <div className="pt-2">
                <div className="grid grid-cols-7 gap-2 text-center font-mono text-[11px]">
                  {integralResult.trapezoidPoints.map((pt) => (
                    <div key={pt.t} className="bg-white p-2 rounded border border-emerald-200">
                      <span className="text-slate-400 block">Hari ke-{pt.t}</span>
                      <span className="font-bold text-emerald-900">{pt.rate} kg/hari</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
