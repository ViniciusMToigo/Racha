using Microsoft.EntityFrameworkCore;

namespace Racha.Infrastructure.Persistence;

public static class RachaDbContextOptions
{
    /// <summary>
    /// Single place for the provider settings, shared by the app, the design-time factory (migrations)
    /// and the integration tests, so all three build exactly the same model.
    /// </summary>
    public static DbContextOptionsBuilder UseRachaPostgres(this DbContextOptionsBuilder builder, string connectionString) =>
        builder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
}
