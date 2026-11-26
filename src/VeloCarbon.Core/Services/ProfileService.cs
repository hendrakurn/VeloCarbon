using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public class ProfileService
{
    private readonly IProfileRepository _repo;
    private readonly IUnitOfWork _uow;
    public ProfileService(IProfileRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public Task<List<UserProfile>> GetAllAsync() => _repo.GetAllAsync();
    public Task<UserProfile?> GetAsync(Guid id) => _repo.GetAsync(id);

    public async Task<UserProfile> CreateProfileAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nama profil wajib diisi.");
        var p = new UserProfile { Name = name.Trim() };
        await _repo.AddAsync(p);
        await _uow.SaveChangesAsync();
        return p;
    }

    public async Task<UserProfile> UpdateProfileAsync(Guid profileId, string name)
    {
        var p = await _repo.GetAsync(profileId) ?? throw new DomainException("Profil tidak ditemukan.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nama profil wajib diisi.");
        p.Name = name.Trim();
        await _uow.SaveChangesAsync();
        return p;
    }

    public async Task<Vehicle> AddVehicleAsync(Guid profileId, string name, TransportMode type, FuelType fuel)
    {
        if (await _repo.GetAsync(profileId) == null) throw new DomainException("Profil tidak ditemukan.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nama kendaraan wajib diisi.");
        if (type is not (TransportMode.Car or TransportMode.Motorcycle)) throw new DomainException("Kendaraan pribadi bertipe Car atau Motorcycle.");
        var v = new Vehicle { UserProfileId = profileId, Name = name.Trim(), VehicleType = type, FuelType = fuel };
        await _repo.AddVehicleAsync(v);
        await _uow.SaveChangesAsync();
        return v;
    }

    public async Task<Vehicle> UpdateVehicleAsync(Guid profileId, Guid vehicleId, string name, TransportMode type, FuelType fuel)
    {
        var v = await _repo.GetVehicleAsync(vehicleId) ?? throw new DomainException("Kendaraan tidak ditemukan.");
        if (v.UserProfileId != profileId) throw new DomainException("Kendaraan bukan milik profil ini.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nama kendaraan wajib diisi.");
        if (type is not (TransportMode.Car or TransportMode.Motorcycle)) throw new DomainException("Kendaraan pribadi bertipe Car atau Motorcycle.");
        if ((v.VehicleType != type || v.FuelType != fuel) && await _repo.VehicleHasActivitiesAsync(vehicleId))
            throw new DomainException("Tipe atau bahan bakar kendaraan tidak dapat diubah karena sudah digunakan oleh aktivitas tercatat. Ubah nama saja atau hapus aktivitas terkait di Riwayat terlebih dahulu.");
        v.Name = name.Trim();
        v.VehicleType = type;
        v.FuelType = fuel;
        await _uow.SaveChangesAsync();
        return v;
    }

    public async Task DeleteVehicleAsync(Guid profileId, Guid vehicleId)
    {
        var v = await _repo.GetVehicleAsync(vehicleId) ?? throw new DomainException("Kendaraan tidak ditemukan.");
        if (v.UserProfileId != profileId) throw new DomainException("Kendaraan bukan milik profil ini.");
        if (await _repo.VehicleHasActivitiesAsync(vehicleId))
            throw new DomainException("Kendaraan tidak dapat dihapus karena masih digunakan oleh aktivitas tercatat. Hapus aktivitas terkait di Riwayat terlebih dahulu.");
        await _repo.RemoveVehicleAsync(v);
        await _uow.SaveChangesAsync();
    }
}
