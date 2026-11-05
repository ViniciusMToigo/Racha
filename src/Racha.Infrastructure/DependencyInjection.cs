using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Abstractions.Security;
using Racha.Infrastructure.Persistence;
using Racha.Infrastructure.Persistence.Repositories;
using Racha.Infrastructure.Security;

namespace Racha.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Racha";

    /// <summary>Registers persistence (EF Core + PostgreSQL), repositories, password hashing and JWT issuing.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddSingleton(TimeProvider.System);

        services.AddDbContext<RachaDbContext>(options => options.UseRachaPostgres(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RachaDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<DatabaseSeeder>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience),
                "Jwt:Issuer and Jwt:Audience are required.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations and, optionally, seeds demo data. Meant for development and demos;
    /// in production, migrations would run as a separate, reviewed deployment step.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services, bool seedDemoData, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<RachaDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        if (seedDemoData)
        {
            await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync(cancellationToken);
        }
    }
}
