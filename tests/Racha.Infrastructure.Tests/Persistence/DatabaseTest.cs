using Racha.Domain.Groups;
using Racha.Domain.Users;
using Racha.Infrastructure.Persistence;

namespace Racha.Infrastructure.Tests.Persistence;

/// <summary>
/// Base for integration tests. Tests share one database, so each one creates its own data
/// (unique e-mails, new ids) and never depends on what other tests left behind.
/// Writes and reads use separate contexts, so assertions hit the database, not EF's in-memory cache.
/// </summary>
public abstract class DatabaseTest(PostgresFixture fixture)
{
    protected static readonly CancellationToken Ct = CancellationToken.None;

    protected static readonly DateOnly Today = new(2026, 11, 4);

    protected RachaDbContext NewContext() => fixture.CreateContext();

    protected static User NewUser(string name) =>
        User.Create(name, $"{name.ToLowerInvariant()}-{Guid.NewGuid():N}@test.dev", "hash");

    protected async Task<User[]> SaveUsersAsync(params string[] names)
    {
        var users = names.Select(NewUser).ToArray();

        await using var db = NewContext();
        db.Users.AddRange(users);
        await db.SaveChangesAsync(Ct);

        return users;
    }

    protected async Task<Group> SaveGroupAsync(User creator, params User[] others)
    {
        var group = Group.Create("Test group", creator.Id);
        foreach (var user in others)
        {
            group.AddMember(user.Id);
        }

        await using var db = NewContext();
        db.Groups.Add(group);
        await db.SaveChangesAsync(Ct);

        return group;
    }
}
