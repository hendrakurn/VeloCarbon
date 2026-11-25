'use client';

import Header from '@/components/Header';
import { initialActivities, initialMonthlyTrend } from '@/data/mockData';
import { 
  TrendingDown, 
  Car, 
  Zap, 
  Flame, 
  ArrowUpRight, 
  PlusCircle, 
  Sparkles, 
  AlertCircle,
  Clock,
  Layers
} from 'lucide-react';
import Link from 'next/link';

export default function Dashboard() {
  // Aggregate current September 2026 data
  const mobilityTotal = initialActivities
    .filter(a => a.category === 'Mobility')
    .reduce((sum, a) => sum + a.resultKgCO2e, 0);

  const electricityTotal = initialActivities
    .filter(a => a.category === 'Electricity')
    .reduce((sum, a) => sum + a.resultKgCO2e, 0);

  const totalEmissions = mobilityTotal + electricityTotal;
  const mobilityPercent = Math.round((mobilityTotal / totalEmissions) * 100);
  const electricityPercent = 100 - mobilityPercent;

  return (
    <div className="flex-1 pb-12">
      <Header 
        title="Dashboard Jejak Karbon" 
        subtitle="Ringkasan Emisi Bulan Berjalan: September 2026"
      />

      <div className="p-8 max-w-7xl mx-auto space-y-8">
        {/* KPI Cards Row */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-5">
          {/* Card 1: Total Emission */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm relative overflow-hidden">
            <div className="absolute top-0 right-0 w-24 h-24 bg-emerald-50 rounded-bl-full pointer-events-none -mr-4 -mt-4"></div>
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Total Emisi Bulan Ini</span>
              <span className="p-1.5 bg-emerald-100 text-emerald-800 rounded-lg">
                <Flame className="w-4 h-4" />
              </span>
            </div>
            <div className="flex items-baseline space-x-2">
              <span className="text-3xl font-extrabold text-slate-900">{totalEmissions.toFixed(2)}</span>
              <span className="text-sm font-semibold text-slate-500">kgCO₂e</span>
            </div>
            <div className="mt-3 flex items-center text-xs text-emerald-700 font-medium">
              <TrendingDown className="w-3.5 h-3.5 mr-1 text-emerald-600" />
              <span>Turun 3.7% dibanding Agustus</span>
            </div>
          </div>

          {/* Card 2: Mobility Emission */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Emisi Mobilitas</span>
              <span className="p-1.5 bg-blue-100 text-blue-800 rounded-lg">
                <Car className="w-4 h-4" />
              </span>
            </div>
            <div className="flex items-baseline space-x-2">
              <span className="text-3xl font-extrabold text-slate-900">{mobilityTotal.toFixed(2)}</span>
              <span className="text-sm font-semibold text-slate-500">kgCO₂e</span>
            </div>
            <div className="mt-3 text-xs text-slate-500 flex justify-between">
              <span>{mobilityPercent}% dari total emisi</span>
              <span className="font-semibold text-blue-600">3 Pembelian BBM</span>
            </div>
          </div>

          {/* Card 3: Electricity Emission */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center justify-between text-slate-500 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Emisi Listrik Kos</span>
              <span className="p-1.5 bg-amber-100 text-amber-800 rounded-lg">
                <Zap className="w-4 h-4" />
              </span>
            </div>
            <div className="flex items-baseline space-x-2">
              <span className="text-3xl font-extrabold text-slate-900">{electricityTotal.toFixed(2)}</span>
              <span className="text-sm font-semibold text-slate-500">kgCO₂e</span>
            </div>
            <div className="mt-3 text-xs text-slate-500 flex justify-between">
              <span>{electricityPercent}% dari total emisi</span>
              <span className="font-semibold text-amber-600">Estimasi 4 Alat</span>
            </div>
          </div>

          {/* Card 4: Dominant Contributor */}
          <div className="bg-gradient-to-br from-slate-900 to-slate-800 text-white p-5 rounded-xl border border-slate-700 shadow-md">
            <div className="flex items-center justify-between text-slate-400 mb-2">
              <span className="text-xs font-semibold uppercase tracking-wider">Sumber Emisi Terbesar</span>
              <AlertCircle className="w-4 h-4 text-emerald-400" />
            </div>
            <div className="text-xl font-bold text-white truncate">Air Conditioner (AC)</div>
            <p className="text-xs text-slate-300 mt-1">Menyumbang ~42.3 kgCO₂e (49.8%)</p>
            <div className="mt-3">
              <Link 
                href="/scenarios" 
                className="inline-flex items-center text-xs font-semibold text-emerald-400 hover:text-emerald-300"
              >
                Simulasi pengurangan <ArrowUpRight className="w-3.5 h-3.5 ml-1" />
              </Link>
            </div>
          </div>
        </div>

        {/* Charts & Proportions Row */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Trend Bar Chart Component */}
          <div className="lg:col-span-2 bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center justify-between mb-6">
              <div>
                <h3 className="font-bold text-slate-800 text-base">Tren Emisi 6 Bulan Terakhir</h3>
                <p className="text-xs text-slate-500">Komparasi emisi mobilitas (BBM/Jarak) vs listrik per bulan</p>
              </div>
              <div className="flex items-center space-x-4 text-xs font-semibold">
                <span className="flex items-center gap-1.5 text-blue-700">
                  <span className="w-3 h-3 rounded bg-blue-500"></span> Mobilitas
                </span>
                <span className="flex items-center gap-1.5 text-amber-700">
                  <span className="w-3 h-3 rounded bg-amber-500"></span> Listrik
                </span>
              </div>
            </div>

            {/* Custom SVG Bar Chart */}
            <div className="h-64 flex items-end justify-between gap-4 pt-4 px-2 border-b border-slate-200">
              {initialMonthlyTrend.map((m) => {
                const maxVal = 100;
                const mobHeight = (m.mobility / maxVal) * 100;
                const elecHeight = (m.electricity / maxVal) * 100;

                return (
                  <div key={m.month} className="flex-1 flex flex-col items-center group relative">
                    {/* Tooltip */}
                    <div className="opacity-0 group-hover:opacity-100 transition-opacity absolute -top-12 bg-slate-900 text-white text-[11px] rounded py-1 px-2 pointer-events-none shadow-lg whitespace-nowrap z-20">
                      Total: {m.total.toFixed(1)} kgCO₂e
                    </div>

                    <div className="w-full max-w-[40px] flex flex-col items-center h-48 justify-end gap-1">
                      {/* Electricity Bar */}
                      <div 
                        className="w-full bg-amber-500 hover:bg-amber-600 rounded-t transition-all"
                        style={{ height: `${elecHeight}%` }}
                      ></div>
                      {/* Mobility Bar */}
                      <div 
                        className="w-full bg-blue-500 hover:bg-blue-600 rounded-t transition-all"
                        style={{ height: `${mobHeight}%` }}
                      ></div>
                    </div>
                    <span className="text-[11px] font-medium text-slate-500 mt-2">{m.month}</span>
                  </div>
                );
              })}
            </div>
            <div className="flex justify-between items-center text-[11px] text-slate-400 mt-3 font-mono">
              <span>Satuan: kgCO₂e per bulan kalender</span>
              <span>Modul 6 Agregasi Relasional</span>
            </div>
          </div>

          {/* Quick Actions & Category Share */}
          <div className="space-y-6">
            {/* Category Share Card */}
            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
              <h3 className="font-bold text-slate-800 text-base mb-4">Proporsi Sumber Emisi</h3>
              <div className="space-y-4">
                <div>
                  <div className="flex justify-between text-xs font-semibold mb-1">
                    <span className="text-amber-800 flex items-center gap-1.5">
                      <Zap className="w-3.5 h-3.5 text-amber-500" /> Konsumsi Listrik
                    </span>
                    <span className="text-slate-700">{electricityPercent}% ({electricityTotal.toFixed(1)} kg)</span>
                  </div>
                  <div className="w-full bg-slate-100 h-2.5 rounded-full overflow-hidden">
                    <div className="bg-amber-500 h-full rounded-full" style={{ width: `${electricityPercent}%` }}></div>
                  </div>
                </div>

                <div>
                  <div className="flex justify-between text-xs font-semibold mb-1">
                    <span className="text-blue-800 flex items-center gap-1.5">
                      <Car className="w-3.5 h-3.5 text-blue-500" /> Mobilitas Bahan Bakar
                    </span>
                    <span className="text-slate-700">{mobilityPercent}% ({mobilityTotal.toFixed(1)} kg)</span>
                  </div>
                  <div className="w-full bg-slate-100 h-2.5 rounded-full overflow-hidden">
                    <div className="bg-blue-500 h-full rounded-full" style={{ width: `${mobilityPercent}%` }}></div>
                  </div>
                </div>
              </div>

              <div className="mt-6 pt-4 border-t border-slate-100">
                <div className="text-xs text-slate-500 flex items-start gap-2">
                  <span className="text-emerald-600 font-bold">Tips:</span>
                  <span>Pengurangan 1 jam AC per hari dapat mereduksi emisi hingga ~5.2 kgCO₂e/bulan.</span>
                </div>
              </div>
            </div>

            {/* Quick Input Shortcut Buttons */}
            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
              <h3 className="font-bold text-slate-800 text-sm mb-3">Aksi Cepat</h3>
              <div className="grid grid-cols-2 gap-3">
                <Link
                  href="/activity?tab=fuel"
                  className="flex items-center justify-center gap-1.5 px-3 py-2.5 rounded-lg border border-slate-300 text-xs font-semibold text-slate-700 hover:bg-slate-50 hover:border-slate-400 transition"
                >
                  <PlusCircle className="w-4 h-4 text-emerald-600" /> + Beli BBM
                </Link>
                <Link
                  href="/activity?tab=appliance"
                  className="flex items-center justify-center gap-1.5 px-3 py-2.5 rounded-lg border border-slate-300 text-xs font-semibold text-slate-700 hover:bg-slate-50 hover:border-slate-400 transition"
                >
                  <PlusCircle className="w-4 h-4 text-amber-600" /> + Estimasi Listrik
                </Link>
                <Link
                  href="/scenarios"
                  className="col-span-2 flex items-center justify-center gap-1.5 px-4 py-2.5 rounded-lg bg-emerald-600 text-white text-xs font-bold hover:bg-emerald-700 transition shadow"
                >
                  <Sparkles className="w-4 h-4" /> Buka Simulasi Skenario
                </Link>
              </div>
            </div>
          </div>
        </div>

        {/* Recent Activity Table Snippet */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
            <h3 className="font-bold text-slate-800 text-base">Aktivitas Terakhir (September 2026)</h3>
            <Link href="/history" className="text-xs font-semibold text-emerald-600 hover:text-emerald-700">
              Lihat Semua Riwayat &rarr;
            </Link>
          </div>

          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-600 border-b border-slate-200 uppercase font-semibold">
              <tr>
                <th className="py-3 px-6">Tanggal</th>
                <th className="py-3 px-6">Kategori</th>
                <th className="py-3 px-6">Rincian Catatan</th>
                <th className="py-3 px-6">Nilai Aktivitas</th>
                <th className="py-3 px-6 text-right">Hasil Emisi</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {initialActivities.map((act) => (
                <tr key={act.id} className="hover:bg-slate-50/80">
                  <td className="py-3.5 px-6 font-mono text-slate-600">{act.date}</td>
                  <td className="py-3.5 px-6">
                    <span className={`px-2 py-0.5 rounded font-semibold text-[11px] ${
                      act.category === 'Mobility' 
                        ? 'bg-blue-100 text-blue-700' 
                        : 'bg-amber-100 text-amber-700'
                    }`}>
                      {act.category}
                    </span>
                  </td>
                  <td className="py-3.5 px-6 font-medium text-slate-800">{act.details}</td>
                  <td className="py-3.5 px-6 font-mono text-slate-600">
                    {act.activityValue} {act.unit}
                  </td>
                  <td className="py-3.5 px-6 text-right font-bold text-slate-900 font-mono">
                    {act.resultKgCO2e.toFixed(3)} kgCO₂e
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
