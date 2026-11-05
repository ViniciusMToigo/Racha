using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Abstractions.Security;
using Racha.Domain.Users;
using Testcontainers.PostgreSql;

namespace Racha.Api.Tests;

/// <summary>
/// Hosts the real Expenses API in memory against a throw-away PostgreSQL container (Docker required).
/// The API applies the migrations itself on startup, exactly as it does in Development.
/// </summary>
public sealed class RachaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public Task InitializeAsync() => _database.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    /// <summary>
    /// Creates a user straight in the database and returns an HTTP client already authenticated as them.
    /// Tokens come from the same ITokenService the Auth API uses, so no second API is needed here.
    /// </summary>
    public async Task<ApiUser> CreateUserAsync(string name)
    {
        await using var scope = Services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var user = User.Create(name, $"{name.ToLowerInvariant()}-{Guid.NewGuid():N}@test.dev", "not-used");
        provider.GetRequiredService<IUserRepository>().Add(user);
        await provider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);

        var token = provider.GetRequiredService<ITokenService>().CreateToken(user);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        return new ApiUser(user, client);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Racha", _database.GetConnectionString());
        builder.UseSetting("Database:SeedDemoData", "false");
    }
}

/// <summary>A test user and an HTTP client that sends their access token.</summary>
public sealed record ApiUser(User User, HttpClient Client)
{
    public Guid Id => User.Id;

    public string Email => User.Email;
}

[CollectionDefinition(Name)]
public sealed class RachaApiCollection : ICollectionFixture<RachaApiFactory>
{
    public const string Name = "Racha API";
}
