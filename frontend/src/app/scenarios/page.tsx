'use client';

import { useState } from 'react';
import Header from '@/components/Header';
import { 
  Sparkles, 
  Sliders, 
  ArrowRight, 
  CheckCircle, 
  Award, 
  Leaf, 
  Save, 
  RotateCcw,
  Zap,
  Car
} from 'lucide-react';

export default function ScenariosPage() {
  // Baseline: September 2026
  const baselineMobility = 27.813;
  const baselineElectricity = 57.075;
  const baselineTotal = baselineMobility + baselineElectricity; // 84.888 kgCO2e

  // Scenario Sliders
  const [reduceFuelPercent, setReduceFuelPercent] = useState<number>(10);
  const [reduceDistancePercent, setReduceDistancePercent] = useState<number>(0);
  const [reduceElecPercent, setReduceElecPercent] = useState<number>(15);
  const [reduceAcHoursPercent, setReduceAcHoursPercent] = useState<number>(25);

  const [isSaved, setIsSaved] = useState(false);

  // Compute reductions
  // Mobility reduction from fuel
  const fuelReduction = baselineMobility * (reduceFuelPercent / 100);
  // Electricity reduction: AC is ~42.3 kgCO2e of 57.075
  const acBaseline = 42.3;
  const acReduction = acBaseline * (reduceAcHoursPercent / 100);
  const otherElecReduction = (baselineElectricity - acBaseline) * (reduceElecPercent / 100);
  const elecReduction = acReduction + otherElecReduction;

  const totalReduction = fuelReduction + elecReduction;
  const scenarioTotal = Math.max(0, baselineTotal - totalReduction);
  const reductionPercentage = ((totalReduction / baselineTotal) * 100).toFixed(1);

  const resetDefaults = () => {
    setReduceFuelPercent(10);
    setReduceDistancePercent(0);
    setReduceElecPercent(15);
    setReduceAcHoursPercent(25);
    setIsSaved(false);
  };

  const handleSaveScenario = () => {
    setIsSaved(true);
    setTimeout(() => setIsSaved(false), 4000);
  };

  return (
    <div className="flex-1 pb-16">
      <Header 
        title="Simulasi Skenario & Rekomendasi" 
        subtitle="Eksperimen Perubahan Kebiasaan Tanpa Mengubah Catatan Asli (What-If Analysis)"
      />

      <div className="p-8 max-w-6xl mx-auto space-y-8">
        {/* Saved Alert */}
        {isSaved && (
          <div className="bg-emerald-50 border border-emerald-300 rounded-xl p-4 flex items-center space-x-3 text-emerald-800 text-sm font-semibold animate-in fade-in duration-200">
            <CheckCircle className="w-5 h-5 text-emerald-600" />
            <span>Skenario & daftar rekomendasi berhasil disimpan ke tabel `scenarios` & `scenario_results`!</span>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column: Sliders / Configuration */}
          <div className="lg:col-span-1 bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-6">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 className="font-bold text-slate-800 text-sm flex items-center gap-2">
                <Sliders className="w-4 h-4 text-emerald-600" />
                Parameter Skenario
              </h3>
              <button 
                onClick={resetDefaults}
                className="text-xs text-slate-400 hover:text-slate-600 flex items-center gap-1"
                title="Reset Skenario"
              >
                <RotateCcw className="w-3 h-3" /> Reset
              </button>
            </div>

            {/* Slider 1: Fuel Reduction */}
            <div className="space-y-2">
              <div className="flex justify-between text-xs font-semibold">
                <span className="text-slate-700 flex items-center gap-1">
                  <Car className="w-3.5 h-3.5 text-blue-600" /> Reduksi BBM Liter
                </span>
                <span className="font-mono text-blue-600 bg-blue-50 px-2 py-0.5 rounded">
                  {reduceFuelPercent}%
                </span>
              </div>
              <input 
                type="range" 
                min="0" 
                max="50" 
                step="5"
                value={reduceFuelPercent}
                onChange={(e) => setReduceFuelPercent(parseInt(e.target.value))}
                className="w-full h-1.5 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-blue-600"
              />
              <span className="text-[11px] text-slate-400 block">Mengurangi ~{(baselineMobility * (reduceFuelPercent/100)).toFixed(2)} kgCO₂e</span>
            </div>

            {/* Slider 2: AC Hours */}
            <div className="space-y-2">
              <div className="flex justify-between text-xs font-semibold">
                <span className="text-slate-700 flex items-center gap-1">
                  <Zap className="w-3.5 h-3.5 text-purple-600" /> Reduksi Jam AC Kos
                </span>
                <span className="font-mono text-purple-600 bg-purple-50 px-2 py-0.5 rounded">
                  {reduceAcHoursPercent}%
                </span>
              </div>
              <input 
                type="range" 
                min="0" 
                max="50" 
                step="5"
                value={reduceAcHoursPercent}
                onChange={(e) => setReduceAcHoursPercent(parseInt(e.target.value))}
                className="w-full h-1.5 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-purple-600"
              />
              <span className="text-[11px] text-slate-400 block">Dari 8 jam/hari menjadi ~6 jam/hari</span>
            </div>

            {/* Slider 3: General Electricity */}
            <div className="space-y-2">
              <div className="flex justify-between text-xs font-semibold">
                <span className="text-slate-700 flex items-center gap-1">
                  <Zap className="w-3.5 h-3.5 text-amber-600" /> Reduksi Alat Lainnya
                </span>
                <span className="font-mono text-amber-600 bg-amber-50 px-2 py-0.5 rounded">
                  {reduceElecPercent}%
                </span>
              </div>
              <input 
                type="range" 
                min="0" 
                max="50" 
                step="5"
                value={reduceElecPercent}
                onChange={(e) => setReduceElecPercent(parseInt(e.target.value))}
                className="w-full h-1.5 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-amber-600"
              />
              <span className="text-[11px] text-slate-400 block">Lampu & standby gadget kos</span>
            </div>

            <div className="pt-4 border-t border-slate-100">
              <button
                onClick={handleSaveScenario}
                className="w-full flex items-center justify-center gap-2 py-2.5 bg-emerald-600 text-white rounded-lg text-xs font-bold hover:bg-emerald-700 transition shadow"
              >
                <Save className="w-4 h-4" /> Simpan Skenario Ini
              </button>
            </div>
          </div>

          {/* Right Column: Comparative Results & Recommendations */}
          <div className="lg:col-span-2 space-y-6">
            {/* Comparison Metric Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
                <span className="text-xs text-slate-500 font-semibold block uppercase">Baseline (Kondisi Awal)</span>
                <span className="text-2xl font-extrabold text-slate-700 font-mono mt-1 block">
                  {baselineTotal.toFixed(2)}
                </span>
                <span className="text-xs text-slate-400">kgCO₂e (Sep 2026)</span>
              </div>

              <div className="bg-emerald-50 border border-emerald-200 p-5 rounded-xl shadow-sm">
                <span className="text-xs text-emerald-800 font-semibold block uppercase">Hasil Skenario</span>
                <span className="text-2xl font-extrabold text-emerald-800 font-mono mt-1 block">
                  {scenarioTotal.toFixed(2)}
                </span>
                <span className="text-xs text-emerald-600">kgCO₂e target baru</span>
              </div>

              <div className="bg-gradient-to-br from-emerald-600 to-teal-700 text-white p-5 rounded-xl shadow-md">
                <span className="text-xs text-emerald-100 font-semibold block uppercase">Total Reduksi</span>
                <div className="flex items-baseline space-x-1.5 mt-1">
                  <span className="text-2xl font-black font-mono">-{totalReduction.toFixed(2)}</span>
                  <span className="text-xs font-semibold">kg ({reductionPercentage}%)</span>
                </div>
                <span className="text-[11px] text-emerald-100">Estimasi hemat Rp 65.000/bln</span>
              </div>
            </div>

            {/* Recommendations List (Ranked by Priority impact) */}
            <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm space-y-4">
              <div className="border-b border-slate-100 pb-3 flex items-center justify-between">
                <div>
                  <h3 className="font-bold text-slate-800 text-base flex items-center gap-2">
                    <Sparkles className="w-5 h-5 text-emerald-600" />
                    Rekomendasi Aksi Terprioritas
                  </h3>
                  <p className="text-xs text-slate-500">Diurutkan berdasarkan besaran dampak reduksi emisi tertinggi (Priority 1 = Dampak terbesar)</p>
                </div>
                <span className="px-2.5 py-1 bg-emerald-100 text-emerald-800 text-[11px] font-bold rounded-full">
                  3 Aksi Tersedia
                </span>
              </div>

              <div className="space-y-3">
                {/* Priority 1 */}
                <div className="p-4 rounded-xl border border-emerald-200 bg-emerald-50/40 flex items-start space-x-4">
                  <div className="w-8 h-8 rounded-full bg-emerald-600 text-white font-bold text-xs flex items-center justify-center shrink-0 shadow">
                    #1
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <h4 className="font-bold text-slate-900 text-xs">Atur Timer AC Kamar Kos (Maksimal 6 Jam/Hari)</h4>
                      <span className="font-mono text-xs font-bold text-emerald-700">
                        -{acReduction.toFixed(2)} kgCO₂e/bln
                      </span>
                    </div>
                    <p className="text-xs text-slate-600 mt-1">
                      Gunakan mode timer otomatis saat tidur malam. AC menyumbang hampir 50% emisi kamar kos Anda.
                    </p>
                    <div className="mt-2 flex items-center gap-3 text-[11px] text-emerald-800 font-semibold">
                      <span>Kategori: Listrik</span>
                      <span>•</span>
                      <span>Dampak: Sangat Tinggi</span>
                    </div>
                  </div>
                </div>

                {/* Priority 2 */}
                <div className="p-4 rounded-xl border border-blue-200 bg-blue-50/40 flex items-start space-x-4">
                  <div className="w-8 h-8 rounded-full bg-blue-600 text-white font-bold text-xs flex items-center justify-center shrink-0 shadow">
                    #2
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <h4 className="font-bold text-slate-900 text-xs">Konsolidasi Perjalanan Harian & Jalan Kaki Jarak Dekat</h4>
                      <span className="font-mono text-xs font-bold text-blue-700">
                        -{fuelReduction.toFixed(2)} kgCO₂e/bln
                      </span>
                    </div>
                    <p className="text-xs text-slate-600 mt-1">
                      Gabungkan belanja mingguan dan gunakan jalan kaki untuk jarak &lt; 500 meter ke kampus/warung makan.
                    </p>
                    <div className="mt-2 flex items-center gap-3 text-[11px] text-blue-800 font-semibold">
                      <span>Kategori: Mobilitas</span>
                      <span>•</span>
                      <span>Dampak: Sedang</span>
                    </div>
                  </div>
                </div>

                {/* Priority 3 */}
                <div className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex items-start space-x-4">
                  <div className="w-8 h-8 rounded-full bg-slate-700 text-white font-bold text-xs flex items-center justify-center shrink-0 shadow">
                    #3
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <h4 className="font-bold text-slate-900 text-xs">Cabut Charger & Matikan Lampu Saat Keluar</h4>
                      <span className="font-mono text-xs font-bold text-slate-700">
                        -{otherElecReduction.toFixed(2)} kgCO₂e/bln
                      </span>
                    </div>
                    <p className="text-xs text-slate-600 mt-1">
                      Hindari beban standby listrik dari colokan adaptor laptop dan lampu belajar.
                    </p>
                    <div className="mt-2 flex items-center gap-3 text-[11px] text-slate-500 font-semibold">
                      <span>Kategori: Listrik</span>
                      <span>•</span>
                      <span>Dampak: Ringan</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
