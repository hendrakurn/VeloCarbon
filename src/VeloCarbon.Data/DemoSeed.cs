using Microsoft.EntityFrameworkCore;

namespace VeloCarbon.Data;

// Profil & kendaraan contoh untuk demo/test (UserProfile/Vehicle belum dipetakan).
public static class DemoSeed
{
    public static readonly Guid ProfileId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid VehicleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task EnsureProfileAndVehicleAsync(VeloCarbonDbContext db)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO user_profiles (id, name, unit_preference)
            VALUES ({ProfileId}, 'Demo Homework 8', 'metric')
            ON CONFLICT (id) DO NOTHING;");
        await db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO vehicles (id, user_profile_id, name, vehicle_type, fuel_type)
            VALUES ({VehicleId}, {ProfileId}, 'Motor Demo', 'Motorcycle', 'Gasoline')
            ON CONFLICT (id) DO NOTHING;");
    }
}
