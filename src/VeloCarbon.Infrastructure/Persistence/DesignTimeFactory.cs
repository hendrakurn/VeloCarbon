using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VeloCarbon.Infrastructure.Persistence;

/// <summary>Dipakai hanya oleh `dotnet ef` saat membuat migrasi.</summary>
public class DesignTimeFactory : IDesignTimeDbContextFactory<VeloCarbonDbContext>
{
    public VeloCarbonDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<VeloCarbonDbContext>()
            .UseNpgsql("Host=localhost;Database=velocarbon;Username=postgres;Password=postgres").Options);
}
