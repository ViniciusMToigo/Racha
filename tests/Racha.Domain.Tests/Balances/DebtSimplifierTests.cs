using FluentAssertions;
using Racha.Domain.Balances;
using Racha.Domain.Common;
using static Racha.Domain.Tests.TestData;

namespace Racha.Domain.Tests.Balances;

public class DebtSimplifierTests
{
    [Fact]
    public void Simplify_WhenEveryoneIsSettled_ReturnsNoTransfers()
    {
        var transfers = DebtSimplifier.Simplify(
        [
            new MemberBalance(Alice, 0m),
            new MemberBalance(Bob, 0m),
        ]);

        transfers.Should().BeEmpty();
    }

    [Fact]
    public void Simplify_OneCreditorManyDebtors_EachDebtorPaysTheCreditor()
    {
        var transfers = DebtSimplifier.Simplify(
        [
            new MemberBalance(Alice, 60m),
            new MemberBalance(Bob, -30m),
            new MemberBalance(Carol, -30m),
        ]);

        transfers.Should().BeEquivalentTo(new[]
        {
            new Transfer(Bob, Alice, 30m),
            new Transfer(Carol, Alice, 30m),
        });
    }

    [Fact]
    public void Simplify_ChainOfDebts_CollapsesIntoSingleTransfer()
    {
        // Alice owes Bob 10 and Bob owes Carol 10: Alice can pay Carol directly.
        var transfers = DebtSimplifier.Simplify(
        [
            new MemberBalance(Alice, -10m),
            new MemberBalance(Bob, 0m),
            new MemberBalance(Carol, 10m),
        ]);

        transfers.Should().Equal(new Transfer(Alice, Carol, 10m));
    }

    [Fact]
    public void Simplify_ManyMembers_SettlesEveryBalanceWithAtMostNMinusOneTransfers()
    {
        var balances = new[]
        {
            new MemberBalance(Alice, 40m),
            new MemberBalance(Bob, 10m),
            new MemberBalance(Carol, -35.50m),
            new MemberBalance(Dave, -14.50m),
        };

        var transfers = DebtSimplifier.Simplify(balances);

        transfers.Should().HaveCountLessThanOrEqualTo(balances.Length - 1);
        transfers.Should().OnlyContain(t => t.Amount > 0m);
        foreach (var balance in balances)
        {
            var received = transfers.Where(t => t.ToUserId == balance.UserId).Sum(t => t.Amount);
            var paid = transfers.Where(t => t.FromUserId == balance.UserId).Sum(t => t.Amount);
            (received - paid).Should().Be(balance.Amount, "every balance must be fully settled");
        }
    }

    [Fact]
    public void Simplify_WhenBalancesDoNotAddUpToZero_Throws()
    {
        var act = () => DebtSimplifier.Simplify(
        [
            new MemberBalance(Alice, 10m),
            new MemberBalance(Bob, -5m),
        ]);

        act.Should().Throw<DomainException>();
    }
}
