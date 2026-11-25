using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeloCarbon.Core.Abstractions;
using VeloCarbon.Core.Services;
using VeloCarbon.Infrastructure.Persistence;
using VeloCarbon.Infrastructure.Repositories;

namespace VeloCarbon.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>Mendaftarkan DbContext PostgreSQL, repository, dan seluruh service aplikasi (dipakai WPF dan Web API).</summary>
    public static IServiceCollection AddVeloCarbon(this IServiceCollection s, string connectionString)
    {
        s.AddDbContext<VeloCarbonDbContext>(o => o.UseNpgsql(connectionString));
        s.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<VeloCarbonDbContext>());
        s.AddScoped<IProfileRepository, ProfileRepository>();
        s.AddScoped<IActivityRepository, ActivityRepository>();
        s.AddScoped<IEmissionFactorRepository, EmissionFactorRepository>();
        s.AddScoped<IScenarioRepository, ScenarioRepository>();
        s.AddSingleton<IEmissionCalculatorService, EmissionCalculatorService>();
        s.AddScoped<ProfileService>();
        s.AddScoped<ActivityService>();
        s.AddScoped<EmissionFactorService>();
        s.AddScoped<DashboardService>();
        s.AddScoped<ScenarioService>();
        return s;
    }
}
