'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { 
  LayoutDashboard, 
  PlusCircle, 
  History, 
  Sparkles, 
  BookOpen, 
  UserCircle, 
  Cpu, 
  ShieldCheck,
  Leaf
} from 'lucide-react';

const navItems = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Catat Aktivitas', href: '/activity', icon: PlusCircle },
  { name: 'Riwayat & Koreksi', href: '/history', icon: History },
  { name: 'Simulasi Skenario', href: '/scenarios', icon: Sparkles },
  { name: 'Faktor Emisi', href: '/factors', icon: BookOpen },
  { name: 'Profil & Kendaraan', href: '/profile', icon: UserCircle },
  { name: 'Engine Modul 10-12', href: '/integrations', icon: Cpu },
];

export default function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="w-64 bg-slate-900 text-slate-100 flex flex-col border-r border-slate-800 shadow-xl h-screen sticky top-0">
      {/* Brand Header */}
      <div className="p-5 border-b border-slate-800 flex items-center space-x-3">
        <div className="h-10 w-10 rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-400 flex items-center justify-center shadow-lg shadow-emerald-900/50">
          <Leaf className="w-6 h-6 text-white" />
        </div>
        <div>
          <h1 className="font-bold text-lg tracking-tight text-white flex items-center gap-1.5">
            VeloCarbon
            <span className="text-[10px] bg-emerald-500/20 text-emerald-400 px-1.5 py-0.5 rounded font-mono font-semibold border border-emerald-500/30">MVP</span>
          </h1>
          <p className="text-xs text-slate-400">Personal Carbon Tracker</p>
        </div>
      </div>

      {/* Navigation Links */}
      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        <div className="px-3 py-2 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
          Menu Utama
        </div>
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.name}
              href={item.href}
              className={`flex items-center space-x-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all ${
                isActive
                  ? 'bg-emerald-600 text-white shadow-md shadow-emerald-950 font-semibold'
                  : 'text-slate-300 hover:bg-slate-800/80 hover:text-white'
              }`}
            >
              <Icon className={`w-5 h-5 ${isActive ? 'text-white' : 'text-slate-400'}`} />
              <span>{item.name}</span>
            </Link>
          );
        })}
      </nav>

      {/* System Status / User Footer */}
      <div className="p-4 border-t border-slate-800 bg-slate-950/50">
        <div className="flex items-center justify-between mb-2">
          <span className="text-[11px] font-mono text-slate-400 flex items-center gap-1.5">
            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
            PostgreSQL: Connected
          </span>
          <ShieldCheck className="w-4 h-4 text-emerald-400" />
        </div>
        <div className="bg-slate-900/90 rounded-lg p-2.5 border border-slate-800 flex items-center space-x-2.5">
          <div className="w-7 h-7 rounded-full bg-slate-700 flex items-center justify-center text-xs font-bold text-emerald-300">
            HK
          </div>
          <div className="truncate">
            <div className="text-xs font-medium text-slate-200 truncate">Hendra Kurnia M.</div>
            <div className="text-[10px] text-slate-400 font-mono">Backend Dev (542344)</div>
          </div>
        </div>
      </div>
    </aside>
  );
}
