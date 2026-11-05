using FluentAssertions;
using Racha.Domain.Balances;
using Racha.Domain.Expenses;
using static Racha.Domain.Tests.TestData;

namespace Racha.Domain.Tests.Balances;

public class BalanceCalculatorTests
{
    [Fact]
    public void Calculate_WithNoExpenses_ReturnsZeroForEveryMember()
    {
        var balances = BalanceCalculator.Calculate([Alice, Bob], []);

        balances.Should().Equal(
            new MemberBalance(Alice, 0m),
            new MemberBalance(Bob, 0m));
    }

    [Fact]
    public void Calculate_SingleExpense_PayerIsOwedTheOthersShares()
    {
        var group = GroupWith(Alice, Bob, Carol);
        var dinner = Expense.Create(group, "Dinner", 90m, Today, Alice);

        var balances = BalanceCalculator.Calculate([Alice, Bob, Carol], [dinner]);

        balances.Should().Equal(
            new MemberBalance(Alice, 60m),
            new MemberBalance(Bob, -30m),
            new MemberBalance(Carol, -30m));
    }

    [Fact]
    public void Calculate_MultipleExpenses_NetsThemOut()
    {
        var group = GroupWith(Alice, Bob, Carol);
        var dinner = Expense.Create(group, "Dinner", 90m, Today, Alice); // 30 each
        var taxi = Expense.Create(group, "Taxi", 60m, Today, Bob);       // 20 each

        var balances = BalanceCalculator.Calculate([Alice, Bob, Carol], [dinner, taxi]);

        balances.Should().Equal(
            new MemberBalance(Alice, 40m),   // paid 90, owes 50
            new MemberBalance(Bob, 10m),     // paid 60, owes 50
            new MemberBalance(Carol, -50m)); // paid 0,  owes 50
    }

    [Fact]
    public void Calculate_BalancesAlwaysAddUpToZero()
    {
        var group = GroupWith(Alice, Bob, Carol);
        var expenses = new[]
        {
            Expense.Create(group, "Groceries", 100m, Today, Alice),
            Expense.Create(group, "Gas", 33.33m, Today, Bob),
            Expense.Create(group, "Snacks", 0.05m, Today, Carol),
        };

        var balances = BalanceCalculator.Calculate([Alice, Bob, Carol], expenses);

        balances.Sum(b => b.Amount).Should().Be(0m);
    }

    [Fact]
    public void Calculate_ReturnsBalancesOrderedByUserId()
    {
        var balances = BalanceCalculator.Calculate([Carol, Alice, Bob], []);

        balances.Select(b => b.UserId).Should().Equal(Alice, Bob, Carol);
    }
}
