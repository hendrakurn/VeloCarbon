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
}
