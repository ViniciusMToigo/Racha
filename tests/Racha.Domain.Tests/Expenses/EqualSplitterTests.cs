using FluentAssertions;
using Racha.Domain.Common;
using Racha.Domain.Expenses;
using static Racha.Domain.Tests.TestData;

namespace Racha.Domain.Tests.Expenses;

public class EqualSplitterTests
{
    [Fact]
    public void Split_EvenAmount_GivesEveryoneTheSameShare()
    {
        var shares = EqualSplitter.Split(90m, [Alice, Bob, Carol]);

        shares.Should().BeEquivalentTo(new[]
        {
            new Share(Alice, 30m),
            new Share(Bob, 30m),
            new Share(Carol, 30m),
        });
    }

    [Fact]
    public void Split_UnevenAmount_GivesLeftoverCentToLowestId()
    {
        // Input order must not matter: the result is ordered by user id.
        var shares = EqualSplitter.Split(100m, [Carol, Alice, Bob]);

        shares.Should().Equal(
            new Share(Alice, 33.34m),
            new Share(Bob, 33.33m),
            new Share(Carol, 33.33m));
    }

    [Fact]
    public void Split_TwoLeftoverCents_GoToTwoLowestIds()
    {
        var shares = EqualSplitter.Split(0.05m, [Alice, Bob, Carol]);

        shares.Should().Equal(
            new Share(Alice, 0.02m),
            new Share(Bob, 0.02m),
            new Share(Carol, 0.01m));
    }

    [Fact]
    public void Split_SingleParticipant_GetsFullAmount()
    {
        var shares = EqualSplitter.Split(42.42m, [Alice]);

        shares.Should().Equal(new Share(Alice, 42.42m));
    }

    [Theory]
    [InlineData("100.00", 3)]
    [InlineData("0.01", 3)]
    [InlineData("1000.01", 7)]
    [InlineData("999999.99", 4)]
    public void Split_SharesAlwaysAddUpToTotalAndDifferByAtMostOneCent(string amount, int participants)
    {
        var total = Dec(amount);
        var ids = Enumerable.Range(1, participants).Select(UserId);

        var shares = EqualSplitter.Split(total, ids);

        shares.Should().HaveCount(participants);
        shares.Sum(s => s.Amount).Should().Be(total);
        (shares.Max(s => s.Amount) - shares.Min(s => s.Amount)).Should().BeLessThanOrEqualTo(0.01m);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("10.001")]
    public void Split_InvalidAmount_Throws(string amount)
    {
        var act = () => EqualSplitter.Split(Dec(amount), [Alice, Bob]);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Split_NoParticipants_Throws()
    {
        var act = () => EqualSplitter.Split(10m, []);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Split_DuplicateParticipants_Throws()
    {
        var act = () => EqualSplitter.Split(10m, [Alice, Alice]);

        act.Should().Throw<DomainException>();
    }
}
