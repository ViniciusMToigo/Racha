using FluentAssertions;
using NSubstitute;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Balances;
using Racha.Application.Common.Exceptions;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;
using static Racha.Application.Tests.TestData;

namespace Racha.Application.Tests.Balances;

public class BalanceServiceTests
{
    private readonly IGroupRepository _groups = Substitute.For<IGroupRepository>();
    private readonly IExpenseRepository _expenses = Substitute.For<IExpenseRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly BalanceService _sut;
    private readonly Group _group;

    public BalanceServiceTests()
    {
        _group = GroupOf(Alice, Bob, Carol);
        _groups.GetByIdAsync(_group.Id, Ct).Returns(_group);
        _users.ResolvesKnownUsers();
        _sut = new BalanceService(_groups, _expenses, _users);
    }

    [Fact]
    public async Task GetAsync_ReturnsNamedBalancesAndSuggestedTransfers()
    {
        var dinner = Expense.Create(_group, "Dinner", 90m, Today, Alice.Id);
        _expenses.GetByGroupAsync(_group.Id, Ct).Returns(new[] { dinner });

        var result = await _sut.GetAsync(Bob.Id, _group.Id, Ct);

        result.GroupId.Should().Be(_group.Id);
        result.Balances.Should().BeEquivalentTo(new[]
        {
            new MemberBalanceDto(Alice.Id, "Alice", 60m),
            new MemberBalanceDto(Bob.Id, "Bob", -30m),
            new MemberBalanceDto(Carol.Id, "Carol", -30m),
        });
        result.SuggestedTransfers.Should().BeEquivalentTo(new[]
        {
            new TransferDto(Bob.Id, "Bob", Alice.Id, "Alice", 30m),
            new TransferDto(Carol.Id, "Carol", Alice.Id, "Alice", 30m),
        });
    }

    [Fact]
    public async Task GetAsync_WithNoExpenses_ReturnsZeroBalancesAndNoTransfers()
    {
        _expenses.GetByGroupAsync(_group.Id, Ct).Returns(Array.Empty<Expense>());

        var result = await _sut.GetAsync(Alice.Id, _group.Id, Ct);

        result.Balances.Should().HaveCount(3).And.OnlyContain(b => b.Amount == 0m);
        result.SuggestedTransfers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_AsNonMember_ThrowsNotFound()
    {
        var act = () => _sut.GetAsync(Dave.Id, _group.Id, Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
