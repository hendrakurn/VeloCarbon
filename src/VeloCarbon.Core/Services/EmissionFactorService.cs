using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public class EmissionFactorService
{
    private readonly IEmissionFactorRepository _repo;
    private readonly IUnitOfWork _uow;
    public EmissionFactorService(IEmissionFactorRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public Task<List<EmissionFactor>> GetAllAsync() => _repo.GetAllAsync();

    /// <summary>Faktor yang cocok kategori+subkategori+satuan+tanggal. Tidak ada / ambigu = error (bukan nol).</summary>
    public static EmissionFactor Resolve(IEnumerable<EmissionFactor> all, ActivityRecord activity)
    {
        var matches = all.Where(f => f.Category == activity.Category
                                     && string.Equals(f.Subcategory, activity.FactorSubcategory, StringComparison.OrdinalIgnoreCase)
                                     && f.Unit == activity.Unit
                                     && f.AppliesOn(activity.ReferenceDate)).ToList();
        if (matches.Count == 0)
            throw new DomainException($"Faktor emisi untuk {activity.Category}/{activity.FactorSubcategory} pada {activity.ReferenceDate:yyyy-MM-dd} tidak ditemukan.");
        if (matches.Count > 1)
            throw new DomainException($"Faktor emisi untuk {activity.Category}/{activity.FactorSubcategory} pada {activity.ReferenceDate:yyyy-MM-dd} ambigu ({matches.Count} faktor aktif).");
        return matches[0];
    }

    /// <summary>Faktor lama tidak ditimpa: versi baru ditambahkan dan versi lama ditutup sehari sebelum ValidFrom baru.</summary>
    public async Task<EmissionFactor> AddNewVersionAsync(EmissionFactor newFactor)
    {
        if (newFactor.Value < 0) throw new DomainException("Nilai faktor emisi tidak boleh negatif.");
        if (string.IsNullOrWhiteSpace(newFactor.Source)) throw new DomainException("Sumber faktor emisi wajib diisi.");
        if (string.IsNullOrWhiteSpace(newFactor.Version)) throw new DomainException("Versi faktor emisi wajib diisi.");
        var all = await _repo.GetAllAsync();
        foreach (var old in all.Where(f => f.IsActive && f.Category == newFactor.Category
                     && f.Subcategory.Equals(newFactor.Subcategory, StringComparison.OrdinalIgnoreCase)
                     && f.Unit == newFactor.Unit && f.ValidUntil == null))
        {
            if (old.ValidFrom.Date >= newFactor.ValidFrom.Date)
                throw new DomainException("Berlaku-mulai versi baru harus setelah versi aktif sebelumnya.");
            old.ValidUntil = newFactor.ValidFrom.Date.AddDays(-1);
        }
        await _repo.AddAsync(newFactor);
        await _uow.SaveChangesAsync();
        return newFactor;
    }
}
