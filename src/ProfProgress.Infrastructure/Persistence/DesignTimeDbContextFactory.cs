using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProfProgress.Infrastructure.Persistence;

/// <summary>
/// `dotnet ef` buyruqlari uchun design-time DbContext yaratuvchi.
/// Ulanish satrini PROFPROGRESS_CONNECTION env-dan oladi, bo'lmasa default'dan.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("PROFPROGRESS_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=profprogress;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}