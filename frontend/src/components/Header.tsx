'use client';

import { Calendar, Bell, Shield, Info } from 'lucide-react';

interface HeaderProps {
  title: string;
  subtitle?: string;
  selectedMonth?: string;
  onMonthChange?: (month: string) => void;
}

export default function Header({
  title,
  subtitle = 'Aplikasi Desktop Estimasi Jejak Karbon Pribadi & Kos',
  selectedMonth = '2026-09',
}: HeaderProps) {
  return (
    <header className="bg-white border-b border-slate-200 px-8 py-4 flex items-center justify-between sticky top-0 z-10 shadow-sm">
      <div>
        <h2 className="text-xl font-bold text-slate-800 tracking-tight">{title}</h2>
        <p className="text-xs text-slate-500">{subtitle}</p>
      </div>

      <div className="flex items-center space-x-4">
        {/* Active Month Selector */}
        <div className="flex items-center space-x-2 bg-slate-50 border border-slate-300 rounded-lg px-3 py-1.5 shadow-inner">
          <Calendar className="w-4 h-4 text-emerald-600" />
          <span className="text-xs font-semibold text-slate-700">Periode:</span>
          <select 
            defaultValue={selectedMonth}
            className="text-xs font-bold text-emerald-700 bg-transparent focus:outline-none cursor-pointer"
          >
            <option value="2026-09">September 2026</option>
            <option value="2026-08">Agustus 2026</option>
            <option value="2026-07">Juli 2026</option>
          </select>
        </div>

        {/* Audit Mode Tag */}
        <div className="hidden md:flex items-center space-x-1.5 px-2.5 py-1 bg-amber-50 text-amber-700 border border-amber-200 rounded-md text-xs font-medium">
          <Info className="w-3.5 h-3.5 text-amber-600" />
          <span>Estimasi Edukatif</span>
        </div>

        {/* Unit Indicator */}
        <div className="px-2.5 py-1 bg-slate-100 border border-slate-200 rounded-md text-xs font-mono font-semibold text-slate-700">
          Unit: kgCO₂e
        </div>
      </div>
    </header>
  );
}
