using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Application.Expenses;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;
using static Racha.Application.Tests.TestData;

namespace Racha.Application.Tests.Expenses;

public class ExpenseServiceTests
{
    private readonly IExpenseRepository _expenses = Substitute.For<IExpenseRepository>();
    private readonly IGroupRepository _groups = Substitute.For<IGroupRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ExpenseService _sut;
    private readonly Group _group;

    public ExpenseServiceTests()
    {
        _group = GroupOf(Alice, Bob, Carol);
        _groups.GetByIdAsync(_group.Id, Ct).Returns(_group);
        _sut = new ExpenseService(_expenses, _groups, _unitOfWork, new ExpenseRequestValidator());
    }

    private static ExpenseRequest Dinner(decimal amount = 90m) => new("Dinner", amount, Today, Alice.Id);

    private Expense StoredExpense(string description = "Dinner", decimal amount = 90m, DateOnly? date = null)
    {
        var expense = Expense.Create(_group, description, amount, date ?? Today, Alice.Id);
        _expenses.GetByIdAsync(expense.Id, Ct).Returns(expense);
        return expense;
    }

    [Fact]
    public async Task CreateAsync_AsMember_AddsExpenseSplitAmongMembersAndSaves()
    {
        var result = await _sut.CreateAsync(Bob.Id, _group.Id, Dinner(), Ct);

        result.GroupId.Should().Be(_group.Id);
        result.Amount.Should().Be(90m);
        result.PaidByUserId.Should().Be(Alice.Id);
        result.Shares.Should().HaveCount(3).And.OnlyContain(s => s.Amount == 30m);
        _expenses.Received(1).Add(Arg.Is<Expense>(e => e.Id == result.Id));
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task CreateAsync_AsNonMember_ThrowsNotFoundAndSavesNothing()
    {
        var act = () => _sut.CreateAsync(Dave.Id, _group.Id, Dinner(), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
        _expenses.DidNotReceive().Add(Arg.Any<Expense>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAmount_ThrowsValidation()
    {
        var act = () => _sut.CreateAsync(Alice.Id, _group.Id, Dinner(amount: 0m), Ct);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ListAsync_ReturnsGroupExpensesNewestFirst()
    {
        var older = Expense.Create(_group, "Groceries", 30m, Today.AddDays(-2), Bob.Id);
        var newer = Expense.Create(_group, "Dinner", 90m, Today, Alice.Id);
        _expenses.GetByGroupAsync(_group.Id, Ct).Returns(new[] { older, newer });

        var result = await _sut.ListAsync(Carol.Id, _group.Id, Ct);

        result.Select(e => e.Description).Should().Equal("Dinner", "Groceries");
    }

    [Fact]
    public async Task ListAsync_AsNonMember_ThrowsNotFound()
    {
        var act = () => _sut.ListAsync(Dave.Id, _group.Id, Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAsync_AsMember_ReturnsExpenseWithShares()
    {
        var expense = StoredExpense();

        var result = await _sut.GetAsync(Carol.Id, expense.Id, Ct);

        result.Id.Should().Be(expense.Id);
        result.Description.Should().Be("Dinner");
        result.Shares.Select(s => s.UserId).Should().BeEquivalentTo(new[] { Alice.Id, Bob.Id, Carol.Id });
    }

    [Fact]
    public async Task GetAsync_AsNonMember_ThrowsNotFound()
    {
        var expense = StoredExpense();

        var act = () => _sut.GetAsync(Dave.Id, expense.Id, Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAsync_UnknownExpense_ThrowsNotFound()
    {
        var act = () => _sut.GetAsync(Alice.Id, Guid.NewGuid(), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_AsMember_UpdatesExpenseAndSaves()
    {
        var expense = StoredExpense();

        var result = await _sut.UpdateAsync(
            Bob.Id, expense.Id, new ExpenseRequest("Dinner and drinks", 120m, Today, Bob.Id), Ct);

        result.Description.Should().Be("Dinner and drinks");
        result.Amount.Should().Be(120m);
        result.PaidByUserId.Should().Be(Bob.Id);
        result.Shares.Should().OnlyContain(s => s.Amount == 40m);
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task UpdateAsync_AsNonMember_ThrowsNotFound()
    {
        var expense = StoredExpense();

        var act = () => _sut.UpdateAsync(Dave.Id, expense.Id, Dinner(), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_AsMember_RemovesAndSaves()
    {
        var expense = StoredExpense();

        await _sut.DeleteAsync(Carol.Id, expense.Id, Ct);

        _expenses.Received(1).Remove(expense);
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task DeleteAsync_AsNonMember_ThrowsNotFound()
    {
        var expense = StoredExpense();

        var act = () => _sut.DeleteAsync(Dave.Id, expense.Id, Ct);

        await act.Should().ThrowAsync<NotFoundException>();
        _expenses.DidNotReceive().Remove(Arg.Any<Expense>());
    }
}
