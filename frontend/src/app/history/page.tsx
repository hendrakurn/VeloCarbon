'use client';

import { useState } from 'react';
import Header from '@/components/Header';
import { initialActivities, ActivityRecord } from '@/data/mockData';
import { 
  Filter, 
  Search, 
  Trash2, 
  Edit3, 
  FileText, 
  CheckCircle, 
  AlertCircle,
  X,
  ExternalLink
} from 'lucide-react';

export default function HistoryPage() {
  const [activities, setActivities] = useState<ActivityRecord[]>(initialActivities);
  const [filterCategory, setFilterCategory] = useState<string>('All');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedAudit, setSelectedAudit] = useState<ActivityRecord | null>(null);

  const filtered = activities.filter((act) => {
    const matchesCat = filterCategory === 'All' || act.category === filterCategory;
    const matchesSearch = act.details.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          act.method.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCat && matchesSearch;
  });

  const handleDelete = (id: string) => {
    if (confirm('Apakah Anda yakin ingin menghapus catatan aktivitas ini?')) {
      setActivities(activities.filter(a => a.id !== id));
    }
  };

  return (
    <div className="flex-1 pb-16">
      <Header 
        title="Riwayat Aktivitas & Audit Perhitungan" 
        subtitle="Daftar aktivitas tersimpan, pembatalan/koreksi catatan, dan jejak audit faktor emisi"
      />

      <div className="p-8 max-w-6xl mx-auto space-y-6">
        {/* Filter and Search Bar */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex items-center space-x-3 w-full md:w-auto">
            <div className="flex items-center space-x-1.5 text-xs font-semibold text-slate-500">
              <Filter className="w-4 h-4 text-emerald-600" />
              <span>Filter:</span>
            </div>
            <select
              value={filterCategory}
              onChange={(e) => setFilterCategory(e.target.value)}
              className="text-xs rounded-lg border border-slate-300 px-3 py-2 bg-slate-50 text-slate-700 focus:outline-none"
            >
              <option value="All">Semua Kategori</option>
              <option value="Mobility">Mobilitas Saja</option>
              <option value="Electricity">Listrik Saja</option>
            </select>
          </div>

          <div className="relative w-full md:w-72">
            <Search className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
            <input
              type="text"
              placeholder="Cari aktivitas atau metode..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-3 py-2 text-xs rounded-lg border border-slate-300 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
            />
          </div>
        </div>

        {/* Activities Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Tanggal</th>
                  <th className="py-3 px-4">Kategori</th>
                  <th className="py-3 px-4">Metode Input</th>
                  <th className="py-3 px-4">Deskripsi Aktivitas</th>
                  <th className="py-3 px-4">Nilai Tercatat</th>
                  <th className="py-3 px-4 text-right">Hasil Emisi</th>
                  <th className="py-3 px-4 text-center">Audit</th>
                  <th className="py-3 px-4 text-center">Aksi</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filtered.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="py-8 text-center text-slate-400">
                      Tidak ada catatan aktivitas yang cocok dengan filter.
                    </td>
                  </tr>
                ) : (
                  filtered.map((act) => (
                    <tr key={act.id} className="hover:bg-slate-50/80 transition">
                      <td className="py-3.5 px-4 font-mono text-slate-600">{act.date}</td>
                      <td className="py-3.5 px-4">
                        <span className={`px-2 py-0.5 rounded font-semibold text-[11px] ${
                          act.category === 'Mobility'
                            ? 'bg-blue-100 text-blue-700'
                            : 'bg-amber-100 text-amber-700'
                        }`}>
                          {act.category}
                        </span>
                      </td>
                      <td className="py-3.5 px-4 font-mono text-slate-600">{act.method}</td>
                      <td className="py-3.5 px-4 font-medium text-slate-800">{act.details}</td>
                      <td className="py-3.5 px-4 font-mono text-slate-700 font-semibold">
                        {act.activityValue} {act.unit}
                      </td>
                      <td className="py-3.5 px-4 text-right font-bold text-slate-900 font-mono">
                        {act.resultKgCO2e.toFixed(3)} kgCO₂e
                      </td>
                      <td className="py-3.5 px-4 text-center">
                        <button
                          onClick={() => setSelectedAudit(act)}
                          className="inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-600 hover:text-emerald-800 bg-emerald-50 px-2 py-1 rounded border border-emerald-200"
                        >
                          <FileText className="w-3 h-3" /> Audit
                        </button>
                      </td>
                      <td className="py-3.5 px-4 text-center">
                        <div className="flex items-center justify-center space-x-1">
                          <button 
                            className="p-1 text-slate-400 hover:text-blue-600 transition"
                            title="Edit Catatan"
                          >
                            <Edit3 className="w-3.5 h-3.5" />
                          </button>
                          <button 
                            onClick={() => handleDelete(act.id)}
                            className="p-1 text-slate-400 hover:text-red-600 transition"
                            title="Hapus Catatan"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Audit Details Modal */}
        {selectedAudit && (
          <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-2xl max-w-lg w-full p-6 shadow-2xl border border-slate-200 animate-in zoom-in-95 duration-150">
              <div className="flex items-center justify-between pb-4 border-b border-slate-100">
                <div className="flex items-center space-x-2">
                  <div className="p-2 bg-emerald-100 text-emerald-700 rounded-lg">
                    <FileText className="w-5 h-5" />
                  </div>
                  <div>
                    <h3 className="font-bold text-slate-800 text-base">Jejak Audit Perhitungan</h3>
                    <p className="text-xs text-slate-400">ID: {selectedAudit.id}</p>
                  </div>
                </div>
                <button
                  onClick={() => setSelectedAudit(null)}
                  className="text-slate-400 hover:text-slate-700 p-1 rounded-lg"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              <div className="py-4 space-y-3 text-xs">
                <div className="bg-slate-50 p-3.5 rounded-xl border border-slate-200 space-y-2">
                  <div className="flex justify-between">
                    <span className="text-slate-500">Nilai Aktivitas (A):</span>
                    <span className="font-mono font-bold text-slate-800">{selectedAudit.activityValue} {selectedAudit.unit}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-500">Faktor Emisi (EF):</span>
                    <span className="font-mono font-bold text-slate-800">{selectedAudit.emissionFactorValue} kgCO₂e/{selectedAudit.unit}</span>
                  </div>
                  <div className="pt-2 border-t border-slate-200 flex justify-between font-bold text-emerald-700">
                    <span>Hasil (A × EF):</span>
                    <span className="font-mono text-sm">{selectedAudit.resultKgCO2e.toFixed(4)} kgCO₂e</span>
                  </div>
                </div>

                <div className="space-y-1.5 pt-2">
                  <div className="flex justify-between text-slate-600">
                    <span>Sumber Dokumen:</span>
                    <span className="font-medium text-slate-800">KLHK RI / Bauran PLN 2023</span>
                  </div>
                  <div className="flex justify-between text-slate-600">
                    <span>Versi Faktor:</span>
                    <span className="font-mono text-slate-800">2024.1 (Active)</span>
                  </div>
                  <div className="flex justify-between text-slate-600">
                    <span>Penyimpanan:</span>
                    <span className="font-mono text-slate-800">PostgreSQL (emission_calculations)</span>
                  </div>
                </div>
              </div>

              <div className="mt-4 pt-3 border-t border-slate-100 flex justify-end">
                <button
                  onClick={() => setSelectedAudit(null)}
                  className="px-4 py-2 bg-slate-800 text-white rounded-lg text-xs font-semibold hover:bg-slate-900"
                >
                  Tutup Audit
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
