using FluentAssertions;
using Racha.Domain.Common;
using Racha.Domain.Expenses;
using static Racha.Domain.Tests.TestData;

namespace Racha.Domain.Tests.Expenses;

public class ExpenseTests
{
    [Fact]
    public void Create_SplitsAmountEquallyAmongGroupMembers()
    {
        var group = GroupWith(Alice, Bob, Carol);

        var expense = Expense.Create(group, "Dinner", 90m, Today, Alice);

        expense.Id.Should().NotBeEmpty();
        expense.GroupId.Should().Be(group.Id);
        expense.Description.Should().Be("Dinner");
        expense.Amount.Should().Be(90m);
        expense.Date.Should().Be(Today);
        expense.PaidByUserId.Should().Be(Alice);
        expense.Shares.Should().HaveCount(3)
            .And.OnlyContain(s => s.Amount == 30m && s.ExpenseId == expense.Id);
        expense.Shares.Select(s => s.UserId).Should().BeEquivalentTo(new[] { Alice, Bob, Carol });
    }

    [Fact]
    public void Create_UnevenAmount_SharesAddUpToTotal()
    {
        var group = GroupWith(Alice, Bob, Carol);

        var expense = Expense.Create(group, "Groceries", 100m, Today, Bob);

        expense.Shares.Sum(s => s.Amount).Should().Be(100m);
        expense.Shares.Single(s => s.UserId == Alice).Amount.Should().Be(33.34m);
    }

    [Fact]
    public void Create_TrimsDescription()
    {
        var group = GroupWith(Alice, Bob);

        var expense = Expense.Create(group, "  Taxi  ", 20m, Today, Alice);

        expense.Description.Should().Be("Taxi");
    }

    [Fact]
    public void Create_WhenPayerIsNotMember_Throws()
    {
        var group = GroupWith(Alice, Bob);

        var act = () => Expense.Create(group, "Dinner", 90m, Today, Dave);

        act.Should().Throw<DomainException>().WithMessage("*member*");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("10.001")]
    public void Create_WithInvalidAmount_Throws(string amount)
    {
        var group = GroupWith(Alice, Bob);

        var act = () => Expense.Create(group, "Dinner", Dec(amount), Today, Alice);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankDescription_Throws(string description)
    {
        var group = GroupWith(Alice, Bob);

        var act = () => Expense.Create(group, description, 10m, Today, Alice);

        act.Should().Throw<DomainException>().WithMessage("*description*");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_Throws()
    {
        var group = GroupWith(Alice, Bob);
        var description = new string('a', Expense.MaxDescriptionLength + 1);

        var act = () => Expense.Create(group, description, 10m, Today, Alice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_ChangesValuesAndRecalculatesShares()
    {
        var group = GroupWith(Alice, Bob, Carol);
        var expense = Expense.Create(group, "Dinner", 90m, Today, Alice);
        var newDate = Today.AddDays(-1);

        expense.Update(group, "Dinner and drinks", 120m, newDate, Bob);

        expense.Description.Should().Be("Dinner and drinks");
        expense.Amount.Should().Be(120m);
        expense.Date.Should().Be(newDate);
        expense.PaidByUserId.Should().Be(Bob);
        expense.Shares.Should().HaveCount(3).And.OnlyContain(s => s.Amount == 40m);
    }

    [Fact]
    public void Update_AfterNewMemberJoins_IncludesNewMemberInSplit()
    {
        var group = GroupWith(Alice, Bob);
        var expense = Expense.Create(group, "Rent", 100m, Today, Alice);
        group.AddMember(Carol);

        expense.Update(group, "Rent", 90m, Today, Alice);

        expense.Shares.Select(s => s.UserId).Should().BeEquivalentTo(new[] { Alice, Bob, Carol });
        expense.Shares.Should().OnlyContain(s => s.Amount == 30m);
    }

    [Fact]
    public void Update_WithDifferentGroup_Throws()
    {
        var group = GroupWith(Alice, Bob);
        var otherGroup = GroupWith(Alice, Bob);
        var expense = Expense.Create(group, "Dinner", 90m, Today, Alice);

        var act = () => expense.Update(otherGroup, "Dinner", 90m, Today, Alice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_WhenPayerIsNotMember_Throws()
    {
        var group = GroupWith(Alice, Bob);
        var expense = Expense.Create(group, "Dinner", 90m, Today, Alice);

        var act = () => expense.Update(group, "Dinner", 90m, Today, Dave);

        act.Should().Throw<DomainException>();
    }
}
