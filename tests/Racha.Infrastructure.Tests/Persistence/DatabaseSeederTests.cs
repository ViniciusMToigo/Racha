using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Racha.Infrastructure.Persistence;
using Racha.Infrastructure.Security;

namespace Racha.Infrastructure.Tests.Persistence;

[Collection(PostgresCollection.Name)]
public class DatabaseSeederTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    private readonly PasswordHasher _hasher = new();

    private async Task SeedAsync()
    {
        await using var db = NewContext();
        await new DatabaseSeeder(db, _hasher, TimeProvider.System).SeedAsync(Ct);
    }

    [Fact]
    public async Task SeedAsync_CreatesDemoUsersWithTheDocumentedPassword()
    {
        await SeedAsync();

        await using var db = NewContext();
        var alice = await db.Users.SingleAsync(u => u.Email == DemoData.AliceEmail, Ct);
        _hasher.Verify(DemoData.Password, alice.PasswordHash).Should().BeTrue();
        (await db.Users.AnyAsync(u => u.Email == DemoData.BobEmail, Ct)).Should().BeTrue();
        (await db.Users.AnyAsync(u => u.Email == DemoData.CarolEmail, Ct)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_CreatesTheTripGroupWithAllThreeMembersAndSomeExpenses()
    {
        await SeedAsync();

        await using var db = NewContext();
        var trip = await db.Groups.Include(g => g.Members).SingleAsync(g => g.Name == DemoData.TripGroupName, Ct);
        trip.Members.Should().HaveCount(3);
        (await db.Expenses.CountAsync(e => e.GroupId == trip.Id, Ct)).Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateData()
    {
        await SeedAsync();
        await SeedAsync();

        await using var db = NewContext();
        (await db.Users.CountAsync(u => u.Email == DemoData.AliceEmail, Ct)).Should().Be(1);
        (await db.Groups.CountAsync(g => g.Name == DemoData.TripGroupName, Ct)).Should().Be(1);
    }
}
