using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Racha.Infrastructure;
using Testcontainers.PostgreSql;

namespace Racha.Auth.Api.Tests;

/// <summary>
/// Hosts the real Auth API in memory against a throw-away PostgreSQL container (Docker required).
/// The Expenses API owns the schema, so here the tests apply the migrations themselves.
/// </summary>
public sealed class AuthApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        await Services.InitializeDatabaseAsync(seedDemoData: false);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Racha", _database.GetConnectionString());
    }
}

[CollectionDefinition(Name)]
public sealed class AuthApiCollection : ICollectionFixture<AuthApiFactory>
{
    public const string Name = "Auth API";
}
