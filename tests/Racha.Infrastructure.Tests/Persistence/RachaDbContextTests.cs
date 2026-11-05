using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Racha.Infrastructure.Persistence;

namespace Racha.Infrastructure.Tests.Persistence;

[Collection(PostgresCollection.Name)]
public class RachaDbContextTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task SaveChanges_StampsCreatedAtOnNewEntities()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-5);

        var users = await SaveUsersAsync("Alice");

        await using var db = NewContext();
        var createdAt = await db.Users
            .Where(u => u.Id == users[0].Id)
            .Select(u => EF.Property<DateTimeOffset>(u, RachaDbContext.CreatedAtColumn))
            .SingleAsync(Ct);

        createdAt.Should().BeAfter(before).And.BeBefore(DateTimeOffset.UtcNow.AddSeconds(5));
    }
}
