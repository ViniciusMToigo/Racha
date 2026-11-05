using Microsoft.EntityFrameworkCore;
using Racha.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Racha.Infrastructure.Tests.Persistence;

/// <summary>
/// Starts one throw-away PostgreSQL container for the whole test run and applies the migrations.
/// Requires Docker to be running.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public RachaDbContext CreateContext()
    {
        var builder = new DbContextOptionsBuilder<RachaDbContext>();
        builder.UseRachaPostgres(_container.GetConnectionString());
        return new RachaDbContext(builder.Options, TimeProvider.System);
    }
}

/// <summary>Every test class in this collection shares the same container.</summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
