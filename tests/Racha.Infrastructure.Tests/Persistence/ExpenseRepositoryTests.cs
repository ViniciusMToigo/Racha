using FluentAssertions;
using Racha.Domain.Expenses;
using Racha.Infrastructure.Persistence.Repositories;

namespace Racha.Infrastructure.Tests.Persistence;

[Collection(PostgresCollection.Name)]
public class ExpenseRepositoryTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Add_ThenGetById_LoadsTheExpenseWithExactAmountsAndShares()
    {
        var users = await SaveUsersAsync("Alice", "Bob", "Carol");
        var group = await SaveGroupAsync(users[0], users[1], users[2]);
        var expense = Expense.Create(group, "Groceries", 100m, Today, users[1].Id);

        await using (var db = NewContext())
        {
            new ExpenseRepository(db).Add(expense);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        var loaded = await new ExpenseRepository(read).GetByIdAsync(expense.Id, Ct);

        loaded.Should().NotBeNull();
        loaded!.Description.Should().Be("Groceries");
        loaded.Amount.Should().Be(100m);
        loaded.Date.Should().Be(Today);
        loaded.PaidByUserId.Should().Be(users[1].Id);
        loaded.Shares.Should().HaveCount(3);
        loaded.Shares.Sum(s => s.Amount).Should().Be(100m, "numeric(12,2) must keep every cent");
    }

    [Fact]
    public async Task Update_AfterANewMemberJoins_ReplacesSharesWithoutDuplicates()
    {
        var users = await SaveUsersAsync("Alice", "Bob", "Carol");
        var group = await SaveGroupAsync(users[0], users[1]);
        var expense = Expense.Create(group, "Rent", 100m, Today, users[0].Id);
        await using (var db = NewContext())
        {
            new ExpenseRepository(db).Add(expense);
            await db.SaveChangesAsync(Ct);
        }

        await using (var db = NewContext())
        {
            var loadedGroup = await new GroupRepository(db).GetByIdAsync(group.Id, Ct);
            var loadedExpense = await new ExpenseRepository(db).GetByIdAsync(expense.Id, Ct);
            loadedGroup!.AddMember(users[2].Id);
            loadedExpense!.Update(loadedGroup, "Rent", 90m, Today, users[0].Id);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        var reloaded = await new ExpenseRepository(read).GetByIdAsync(expense.Id, Ct);
        reloaded!.Amount.Should().Be(90m);
        reloaded.Shares.Select(s => s.UserId).Should().BeEquivalentTo(users.Select(u => u.Id));
        reloaded.Shares.Should().OnlyContain(s => s.Amount == 30m);
    }

    [Fact]
    public async Task GetByGroupAsync_ReturnsOnlyThatGroupsExpensesNewestFirst()
    {
        var users = await SaveUsersAsync("Alice", "Bob");
        var group = await SaveGroupAsync(users[0], users[1]);
        var otherGroup = await SaveGroupAsync(users[0], users[1]);

        await using (var db = NewContext())
        {
            var repository = new ExpenseRepository(db);
            repository.Add(Expense.Create(group, "Older", 10m, Today.AddDays(-3), users[0].Id));
            repository.Add(Expense.Create(group, "Newer", 20m, Today, users[1].Id));
            repository.Add(Expense.Create(otherGroup, "Elsewhere", 30m, Today, users[0].Id));
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        var expenses = await new ExpenseRepository(read).GetByGroupAsync(group.Id, Ct);

        expenses.Select(e => e.Description).Should().Equal("Newer", "Older");
        expenses.Should().OnlyContain(e => e.Shares.Count == 2, "shares must be loaded too");
    }

    [Fact]
    public async Task Remove_DeletesTheExpenseAndItsShares()
    {
        var users = await SaveUsersAsync("Alice", "Bob");
        var group = await SaveGroupAsync(users[0], users[1]);
        var expense = Expense.Create(group, "Taxi", 40m, Today, users[0].Id);
        await using (var db = NewContext())
        {
            new ExpenseRepository(db).Add(expense);
            await db.SaveChangesAsync(Ct);
        }

        await using (var db = NewContext())
        {
            var repository = new ExpenseRepository(db);
            repository.Remove((await repository.GetByIdAsync(expense.Id, Ct))!);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        (await new ExpenseRepository(read).GetByIdAsync(expense.Id, Ct)).Should().BeNull();
    }
}
