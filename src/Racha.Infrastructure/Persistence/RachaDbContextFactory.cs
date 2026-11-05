using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Racha.Infrastructure.Persistence;

/// <summary>
/// Used only by the `dotnet ef` tools, so migrations can be created from this project alone
/// without starting an API. Defaults to the docker-compose database.
/// </summary>
public sealed class RachaDbContextFactory : IDesignTimeDbContextFactory<RachaDbContext>
{
    private const string LocalConnectionString =
        "Host=localhost;Port=5432;Database=racha;Username=racha;Password=racha_dev_password";

    public RachaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("RACHA_CONNECTION_STRING") ?? LocalConnectionString;

        var builder = new DbContextOptionsBuilder<RachaDbContext>();
        builder.UseRachaPostgres(connectionString);

        return new RachaDbContext(builder.Options, TimeProvider.System);
    }
}
