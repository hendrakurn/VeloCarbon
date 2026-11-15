'use client';

import { useState } from 'react';
import Header from '@/components/Header';
import { initialEmissionFactors, EmissionFactor } from '@/data/mockData';
import { BookOpen, Search, ShieldCheck, ExternalLink, Info } from 'lucide-react';

export default function FactorsPage() {
  const [factors] = useState<EmissionFactor[]>(initialEmissionFactors);
  const [search, setSearch] = useState('');

  const filtered = factors.filter(f => 
    f.subcategory.toLowerCase().includes(search.toLowerCase()) ||
    f.source.toLowerCase().includes(search.toLowerCase()) ||
    f.category.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="flex-1 pb-16">
      <Header 
        title="Katalog Faktor Emisi (Emission Factors)" 
        subtitle="Daftar faktor konversi emisi terverifikasi dan berversi untuk transparansi audit"
      />

      <div className="p-8 max-w-6xl mx-auto space-y-6">
        {/* Notice Card */}
        <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex items-center justify-between text-xs text-emerald-800">
          <div className="flex items-center space-x-3">
            <ShieldCheck className="w-5 h-5 text-emerald-600 shrink-0" />
            <div>
              <span className="font-bold">Standar Data Terbuka:</span> Seluruh kalkulasi VeloCarbon menggunakan faktor emisi lokal resmi dari publikasi IPCC, KLHK RI, dan Kementerian ESDM / PLN.
            </div>
          </div>
          <span className="px-2.5 py-1 bg-white font-mono text-[11px] rounded border border-emerald-300 font-semibold">
            Status: Semua Aktif
          </span>
        </div>

        {/* Search */}
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
          <div className="relative w-full max-w-md">
            <Search className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
            <input
              type="text"
              placeholder="Cari jenis bahan bakar, moda, atau sumber..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-3 py-2 text-xs rounded-lg border border-slate-300 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
            />
          </div>
          <span className="text-xs text-slate-400 font-mono">Total {filtered.length} Faktor Terdaftar</span>
        </div>

        {/* Factors Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
              <tr>
                <th className="py-3.5 px-4">Kategori</th>
                <th className="py-3.5 px-4">Subkategori / Bahan</th>
                <th className="py-3.5 px-4">Nilai Faktor</th>
                <th className="py-3.5 px-4">Satuan</th>
                <th className="py-3.5 px-4">Sumber Dokumen</th>
                <th className="py-3.5 px-4">Versi</th>
                <th className="py-3.5 px-4">Berlaku Sejak</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-sans">
              {filtered.map((f) => (
                <tr key={f.id} className="hover:bg-slate-50/80 transition">
                  <td className="py-3 px-4">
                    <span className={`px-2 py-0.5 rounded font-semibold text-[11px] ${
                      f.category === 'Mobility'
                        ? 'bg-blue-100 text-blue-700'
                        : 'bg-amber-100 text-amber-700'
                    }`}>
                      {f.category}
                    </span>
                  </td>
                  <td className="py-3 px-4 font-bold text-slate-800">{f.subcategory}</td>
                  <td className="py-3 px-4 font-mono font-extrabold text-emerald-700 text-sm">
                    {f.value}
                  </td>
                  <td className="py-3 px-4 font-mono text-slate-600 font-semibold">{f.unit}</td>
                  <td className="py-3 px-4 text-slate-600 max-w-xs truncate">{f.source}</td>
                  <td className="py-3 px-4 font-mono text-slate-500">{f.version}</td>
                  <td className="py-3 px-4 font-mono text-slate-500">{f.validFrom}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
