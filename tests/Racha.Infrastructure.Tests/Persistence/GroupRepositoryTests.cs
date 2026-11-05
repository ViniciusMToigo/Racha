using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;
using Racha.Infrastructure.Persistence.Repositories;

namespace Racha.Infrastructure.Tests.Persistence;

[Collection(PostgresCollection.Name)]
public class GroupRepositoryTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Add_ThenGetById_LoadsTheGroupWithItsMembers()
    {
        var users = await SaveUsersAsync("Alice", "Bob");
        var group = Group.Create("Trip", users[0].Id);
        group.AddMember(users[1].Id);

        await using (var db = NewContext())
        {
            new GroupRepository(db).Add(group);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        var loaded = await new GroupRepository(read).GetByIdAsync(group.Id, Ct);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Trip");
        loaded.CreatedByUserId.Should().Be(users[0].Id);
        loaded.Members.Select(m => m.UserId).Should().BeEquivalentTo(new[] { users[0].Id, users[1].Id });
    }

    [Fact]
    public async Task GetByIdAsync_UnknownGroup_ReturnsNull()
    {
        await using var db = NewContext();

        var group = await new GroupRepository(db).GetByIdAsync(Guid.NewGuid(), Ct);

        group.Should().BeNull();
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyGroupsTheUserBelongsTo()
    {
        var users = await SaveUsersAsync("Alice", "Bob", "Carol");
        var aliceAndBob = await SaveGroupAsync(users[0], users[1]);
        var bobAndCarol = await SaveGroupAsync(users[1], users[2]);

        await using var db = NewContext();
        var groups = await new GroupRepository(db).GetForUserAsync(users[0].Id, Ct);

        groups.Select(g => g.Id).Should().Equal(aliceAndBob.Id);
        groups.Single().Members.Should().HaveCount(2, "members must be loaded too");
        groups.Select(g => g.Id).Should().NotContain(bobAndCarol.Id);
    }

    [Fact]
    public async Task AddMember_OnALoadedGroup_PersistsTheNewMember()
    {
        var users = await SaveUsersAsync("Alice", "Bob");
        var group = await SaveGroupAsync(users[0]);

        await using (var db = NewContext())
        {
            var loaded = await new GroupRepository(db).GetByIdAsync(group.Id, Ct);
            loaded!.AddMember(users[1].Id);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        var reloaded = await new GroupRepository(read).GetByIdAsync(group.Id, Ct);
        reloaded!.IsMember(users[1].Id).Should().BeTrue();
    }

    [Fact]
    public async Task Remove_DeletesTheGroupWithItsMembersExpensesAndShares()
    {
        var users = await SaveUsersAsync("Alice", "Bob");
        var group = await SaveGroupAsync(users[0], users[1]);
        var expense = Expense.Create(group, "Dinner", 90m, Today, users[0].Id);
        await using (var db = NewContext())
        {
            db.Expenses.Add(expense);
            await db.SaveChangesAsync(Ct);
        }

        await using (var db = NewContext())
        {
            var repository = new GroupRepository(db);
            repository.Remove((await repository.GetByIdAsync(group.Id, Ct))!);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        (await read.Groups.AnyAsync(g => g.Id == group.Id, Ct)).Should().BeFalse();
        (await read.Expenses.AnyAsync(e => e.GroupId == group.Id, Ct)).Should().BeFalse();
        (await read.Users.CountAsync(u => u.Id == users[0].Id || u.Id == users[1].Id, Ct))
            .Should().Be(2, "deleting a group must never delete users");
    }
}
