using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Models;

namespace VeloCarbon.Core.Services;

public static class MonthHelper
{
    public static DateTime MonthStart(DateTime d) => new(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static DateTime MonthEnd(DateTime d) => MonthStart(d).AddMonths(1).AddDays(-1);
}

public class ActivityService
{
    private readonly IActivityRepository _activities;
    private readonly IProfileRepository _profiles;
    private readonly IEmissionFactorRepository _factors;
    private readonly IEmissionCalculatorService _calculator;
    private readonly IUnitOfWork _uow;

    public ActivityService(IActivityRepository activities, IProfileRepository profiles, IEmissionFactorRepository factors,
        IEmissionCalculatorService calculator, IUnitOfWork uow)
    { _activities = activities; _profiles = profiles; _factors = factors; _calculator = calculator; _uow = uow; }

    private static bool IsPrivateVehicleMode(TransportMode m) => m is TransportMode.Car or TransportMode.Motorcycle;

    public async Task<ActivityRecord> AddDistanceAsync(DistanceInput i)
    {
        await RequireProfileAsync(i.ProfileId);
        if (i.DistanceKm <= 0) throw new DomainException("Jarak harus lebih dari 0 km.");
        if (i.Frequency < 1) throw new DomainException("Frekuensi minimal 1 perjalanan.");

        Guid? vehicleId = i.VehicleId;
        if (IsPrivateVehicleMode(i.Mode))
        {
            if (vehicleId == null) throw new DomainException("Perjalanan dengan kendaraan pribadi wajib memilih kendaraan.");
            var v = await RequireVehicleAsync(i.ProfileId, vehicleId.Value);
            if (v.VehicleType != i.Mode) throw new DomainException($"Kendaraan '{v.Name}' bertipe {v.VehicleType}, bukan {i.Mode}.");
            await EnsureVehicleMethodAsync(vehicleId.Value, i.Date, MobilityInputMethod.Distance);
        }
        else if (vehicleId != null)
            throw new DomainException($"Moda {i.Mode} tidak memakai kendaraan pribadi; kosongkan kendaraan.");

        var activity = new MobilityActivity
        {
            UserProfileId = i.ProfileId, VehicleId = vehicleId, ActivityDate = i.Date.Date,
            TransportMode = i.Mode, DistanceKm = i.DistanceKm, Frequency = i.Frequency
        };
        return await SaveWithCalculationAsync(activity);
    }

    public async Task<ActivityRecord> AddFuelPurchaseAsync(FuelPurchaseInput i)
    {
        await RequireProfileAsync(i.ProfileId);
        if (i.Liters <= 0) throw new DomainException("Liter bahan bakar harus lebih dari 0.");
        if (i.TotalCost is < 0) throw new DomainException("Biaya tidak boleh negatif.");
        var v = await RequireVehicleAsync(i.ProfileId, i.VehicleId);
        if (v.VehicleType is not (TransportMode.Car or TransportMode.Motorcycle))
            throw new DomainException("Pembelian bahan bakar hanya untuk kendaraan pribadi bermesin.");
        if (v.FuelType != i.FuelType) throw new DomainException($"Kendaraan '{v.Name}' memakai {v.FuelType}, bukan {i.FuelType}.");
        await EnsureVehicleMethodAsync(i.VehicleId, i.Date, MobilityInputMethod.FuelPurchase);

        var activity = new FuelPurchaseActivity
        {
            UserProfileId = i.ProfileId, VehicleId = i.VehicleId, PurchaseDate = i.Date.Date,
            FuelType = i.FuelType, LitersPurchased = i.Liters, TotalCost = i.TotalCost
        };
        return await SaveWithCalculationAsync(activity);
    }

    public async Task<ActivityRecord> AddMeterElectricityAsync(MeterElectricityInput i)
    {
        await RequireProfileAsync(i.ProfileId);
        if (i.UsageKWh <= 0) throw new DomainException("Total kWh harus lebih dari 0.");
        await EnsureNoElectricityAsync(i.ProfileId, i.Month);
        var e = NewElectricity(i.ProfileId, i.Month, ElectricityInputMethod.MeterKWh);
        e.UsageKWh = i.UsageKWh;
        return await SaveWithCalculationAsync(e);
    }

    public async Task<ActivityRecord> AddApplianceElectricityAsync(ApplianceElectricityInput i)
    {
        await RequireProfileAsync(i.ProfileId);
        if (i.Appliances.Count == 0) throw new DomainException("Tambahkan minimal satu alat elektronik.");
        var daysInMonth = DateTime.DaysInMonth(i.Month.Year, i.Month.Month);
        await EnsureNoElectricityAsync(i.ProfileId, i.Month);
        var e = NewElectricity(i.ProfileId, i.Month, ElectricityInputMethod.ApplianceEstimate);
        foreach (var a in i.Appliances)
        {
            if (string.IsNullOrWhiteSpace(a.Name)) throw new DomainException("Nama alat wajib diisi.");
            if (a.PowerWatt <= 0) throw new DomainException($"Daya '{a.Name}' harus lebih dari 0 watt.");
            if (a.Quantity < 1) throw new DomainException($"Jumlah '{a.Name}' minimal 1.");
            if (a.HoursPerDay <= 0 || a.HoursPerDay > 24) throw new DomainException($"Jam per hari '{a.Name}' harus di antara 0 (tidak termasuk) dan 24.");
            if (a.DaysUsed < 1 || a.DaysUsed > daysInMonth) throw new DomainException($"Hari pemakaian '{a.Name}' harus 1-{daysInMonth}.");
            e.Appliances.Add(new ApplianceUsage { ElectricityUsageId = e.Id, Name = a.Name.Trim(), PowerWatt = a.PowerWatt, Quantity = a.Quantity, HoursPerDay = a.HoursPerDay, DaysUsed = a.DaysUsed });
        }
        e.RecalculateFromAppliances();
        return await SaveWithCalculationAsync(e);
    }

    public async Task DeleteAsync(Guid activityId)
    {
        var a = await _activities.GetAsync(activityId) ?? throw new DomainException("Aktivitas tidak ditemukan.");
        var vehicleId = a switch { MobilityActivity m => m.VehicleId, FuelPurchaseActivity f => f.VehicleId, _ => null };
        await _activities.RemoveAsync(a);
        if (vehicleId != null)
        {
            var month = MonthHelper.MonthStart(a.ReferenceDate);
            if (!await _activities.VehicleMonthHasActivitiesAsync(vehicleId.Value, month, a.Id))
                await _activities.RemoveVehicleMonthAsync(vehicleId.Value, month);
        }
        await _uow.SaveChangesAsync();
    }

    public Task<List<ActivityRecord>> GetMonthAsync(Guid profileId, DateTime month) =>
        _activities.GetByMonthAsync(profileId, MonthHelper.MonthStart(month));

    // ---- helpers ----
    private static ElectricityUsage NewElectricity(Guid profileId, DateTime month, ElectricityInputMethod method) => new()
    {
        UserProfileId = profileId, InputMethod = method,
        PeriodStart = MonthHelper.MonthStart(month), PeriodEnd = MonthHelper.MonthEnd(month)
    };

    private async Task RequireProfileAsync(Guid profileId)
    {
        if (await _profiles.GetAsync(profileId) == null) throw new DomainException("Profil tidak ditemukan.");
    }

    private async Task<Vehicle> RequireVehicleAsync(Guid profileId, Guid vehicleId)
    {
        var v = await _profiles.GetVehicleAsync(vehicleId) ?? throw new DomainException("Kendaraan tidak ditemukan.");
        if (v.UserProfileId != profileId) throw new DomainException("Kendaraan bukan milik profil ini.");
        return v;
    }

    private async Task EnsureVehicleMethodAsync(Guid vehicleId, DateTime date, MobilityInputMethod method)
    {
        var month = MonthHelper.MonthStart(date);
        var existing = await _activities.GetVehicleMonthAsync(vehicleId, month);
        if (existing == null)
            await _activities.AddVehicleMonthAsync(new VehicleMonthlyInput { VehicleId = vehicleId, Month = month, Method = method });
        else if (existing.Method != method)
            throw new DomainException($"Kendaraan ini pada {month:MMMM yyyy} sudah memakai metode {existing.Method}. Hapus catatan metode tersebut terlebih dahulu agar tidak terhitung ganda.");
    }

    private async Task EnsureNoElectricityAsync(Guid profileId, DateTime month)
    {
        if (await _activities.GetElectricityAsync(profileId, MonthHelper.MonthStart(month)) != null)
            throw new DomainException($"Listrik bulan {month:MMMM yyyy} sudah dicatat. Hapus catatan lama lalu input ulang agar tidak terhitung ganda.");
    }

    private async Task<ActivityRecord> SaveWithCalculationAsync(ActivityRecord activity)
    {
        var factor = EmissionFactorService.Resolve(await _factors.GetAllAsync(), activity);
        activity.Calculation = _calculator.CalculateActivityEmission(activity, factor);
        await _activities.AddAsync(activity);
        await _uow.SaveChangesAsync(); // atomik: aktivitas + hasil emisi + metode bulanan
        return activity;
    }
}
