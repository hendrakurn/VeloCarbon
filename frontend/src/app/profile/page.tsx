'use client';

import { useState } from 'react';
import Header from '@/components/Header';
import { initialProfile, initialVehicles, Vehicle } from '@/data/mockData';
import { User, Car, Plus, Trash2, Save, CheckCircle, Shield } from 'lucide-react';

export default function ProfilePage() {
  const [profile, setProfile] = useState(initialProfile);
  const [vehicles, setVehicles] = useState<Vehicle[]>(initialVehicles);
  const [savedAlert, setSavedAlert] = useState(false);

  // New vehicle form modal state
  const [showAddModal, setShowAddModal] = useState(false);
  const [newVehName, setNewVehName] = useState('');
  const [newVehType, setNewVehType] = useState<'Motorcycle' | 'Car'>('Motorcycle');
  const [newFuelType, setNewFuelType] = useState<'Pertalite' | 'Pertamax' | 'Solar' | 'Electric'>('Pertalite');

  const handleSaveProfile = (e: React.FormEvent) => {
    e.preventDefault();
    setSavedAlert(true);
    setTimeout(() => setSavedAlert(false), 3000);
  };

  const handleAddVehicle = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newVehName) return;
    const newV: Vehicle = {
      id: `veh-${Date.now()}`,
      name: newVehName,
      vehicleType: newVehType,
      fuelType: newFuelType,
    };
    setVehicles([...vehicles, newV]);
    setNewVehName('');
    setShowAddModal(false);
  };

  const handleDeleteVehicle = (id: string) => {
    if (confirm('Hapus kendaraan ini dari daftar Anda?')) {
      setVehicles(vehicles.filter(v => v.id !== id));
    }
  };

  return (
    <div className="flex-1 pb-16">
      <Header 
        title="Profil Pengguna & Kendaraan" 
        subtitle="Kelola identitas pengguna dan armada kendaraan pribadi Anda"
      />

      <div className="p-8 max-w-5xl mx-auto space-y-8">
        {savedAlert && (
          <div className="bg-emerald-50 border border-emerald-300 rounded-xl p-4 flex items-center space-x-3 text-emerald-800 text-sm font-semibold animate-in fade-in">
            <CheckCircle className="w-5 h-5 text-emerald-600" />
            <span>Perubahan profil berhasil disimpan!</span>
          </div>
        )}

        {/* User Profile Card */}
        <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm">
          <div className="flex items-center space-x-3 border-b border-slate-100 pb-4 mb-6">
            <div className="p-2 bg-emerald-100 text-emerald-700 rounded-lg">
              <User className="w-5 h-5" />
            </div>
            <div>
              <h3 className="font-bold text-slate-800 text-base">Informasi Profil</h3>
              <p className="text-xs text-slate-500">Data akun dan preferensi satuan perhitungan</p>
            </div>
          </div>

          <form onSubmit={handleSaveProfile} className="grid grid-cols-1 md:grid-cols-2 gap-5">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">Nama Lengkap</label>
              <input
                type="text"
                value={profile.name}
                onChange={(e) => setProfile({ ...profile, name: e.target.value })}
                className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">NIM / NIU</label>
              <input
                type="text"
                value={profile.nim}
                disabled
                className="w-full text-xs rounded-lg border border-slate-200 bg-slate-50 text-slate-500 p-2.5 font-mono cursor-not-allowed"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">Email Mahasiswa</label>
              <input
                type="email"
                value={profile.email}
                onChange={(e) => setProfile({ ...profile, email: e.target.value })}
                className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1.5">Preferensi Satuan</label>
              <select
                value={profile.unitPreference}
                onChange={(e) => setProfile({ ...profile, unitPreference: e.target.value })}
                className="w-full text-xs rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
              >
                <option value="Metric (km, L, kWh)">Metrik (km, Liter, kWh) — Standar Indonesia</option>
                <option value="Imperial">Imperial (Miles, Gallons, kWh)</option>
              </select>
            </div>

            <div className="md:col-span-2 flex justify-end pt-2">
              <button
                type="submit"
                className="flex items-center space-x-2 px-5 py-2 bg-emerald-600 text-white rounded-lg text-xs font-bold hover:bg-emerald-700 shadow"
              >
                <Save className="w-4 h-4" /> Simpan Profil
              </button>
            </div>
          </form>
        </div>

        {/* Vehicles Management Card */}
        <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm">
          <div className="flex items-center justify-between border-b border-slate-100 pb-4 mb-6">
            <div className="flex items-center space-x-3">
              <div className="p-2 bg-blue-100 text-blue-700 rounded-lg">
                <Car className="w-5 h-5" />
              </div>
              <div>
                <h3 className="font-bold text-slate-800 text-base">Daftar Kendaraan Terdaftar</h3>
                <p className="text-xs text-slate-500">Kendaraan yang digunakan untuk pencatatan perjalanan atau pembelian BBM</p>
              </div>
            </div>

            <button
              onClick={() => setShowAddModal(true)}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 text-white rounded-lg text-xs font-semibold hover:bg-blue-700 shadow"
            >
              <Plus className="w-4 h-4" /> + Tambah Kendaraan
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {vehicles.map((v) => (
              <div 
                key={v.id}
                className="border border-slate-200 rounded-xl p-4 bg-slate-50/50 flex items-center justify-between hover:border-slate-300 transition"
              >
                <div className="flex items-center space-x-3">
                  <div className="w-10 h-10 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-sm">
                    {v.vehicleType === 'Motorcycle' ? '🛵' : '🚗'}
                  </div>
                  <div>
                    <h4 className="font-bold text-slate-900 text-xs">{v.name}</h4>
                    <p className="text-[11px] text-slate-500">
                      Tipe: {v.vehicleType} • BBM: <span className="font-semibold text-emerald-700">{v.fuelType}</span>
                    </p>
                  </div>
                </div>

                <button
                  onClick={() => handleDeleteVehicle(v.id)}
                  className="p-1.5 text-slate-400 hover:text-red-600 rounded-lg transition"
                  title="Hapus Kendaraan"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        </div>

        {/* Add Vehicle Modal */}
        {showAddModal && (
          <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-200">
              <h3 className="font-bold text-slate-800 text-base mb-4">Tambah Kendaraan Baru</h3>
              <form onSubmit={handleAddVehicle} className="space-y-4 text-xs">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Nama / Merk Kendaraan</label>
                  <input
                    type="text"
                    placeholder="Contoh: Yamaha NMAX 155"
                    value={newVehName}
                    onChange={(e) => setNewVehName(e.target.value)}
                    className="w-full rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none"
                    required
                  />
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Tipe Kendaraan</label>
                  <select
                    value={newVehType}
                    onChange={(e) => setNewVehType(e.target.value as any)}
                    className="w-full rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  >
                    <option value="Motorcycle">Sepeda Motor</option>
                    <option value="Car">Mobil Pribadi</option>
                  </select>
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Bahan Bakar Utama</label>
                  <select
                    value={newFuelType}
                    onChange={(e) => setNewFuelType(e.target.value as any)}
                    className="w-full rounded-lg border border-slate-300 p-2.5 focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  >
                    <option value="Pertalite">Pertalite (RON 90)</option>
                    <option value="Pertamax">Pertamax (RON 92)</option>
                    <option value="Solar">Solar / Biosolar</option>
                    <option value="Electric">Listrik (EV)</option>
                  </select>
                </div>

                <div className="flex justify-end space-x-2 pt-3 border-t border-slate-100">
                  <button
                    type="button"
                    onClick={() => setShowAddModal(false)}
                    className="px-4 py-2 border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 font-semibold"
                  >
                    Batal
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-semibold shadow"
                  >
                    Simpan Kendaraan
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
