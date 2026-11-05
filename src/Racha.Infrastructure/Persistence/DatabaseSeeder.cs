using Racha.Application.Abstractions.Security;

namespace Racha.Infrastructure.Persistence;

/// <summary>Demo credentials, also documented in the README.</summary>
public static class DemoData
{
    public const string Password = "Demo123!";
    public const string AliceEmail = "alice@racha.dev";
    public const string BobEmail = "bob@racha.dev";
    public const string CarolEmail = "carol@racha.dev";
    public const string TripGroupName = "Trip to Floripa";
}

// Red phase only: the dependencies are not used yet. Removed when the seeder is implemented.
#pragma warning disable CS9113 // Parameter is unread

/// <summary>Creates demo users, a group and expenses. Safe to run many times: it only seeds once.</summary>
public sealed class DatabaseSeeder(RachaDbContext db, IPasswordHasher passwordHasher, TimeProvider timeProvider)
{
    public Task SeedAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
