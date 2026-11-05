using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Racha.Application.Balances;

namespace Racha.Api.Tests;

[Collection(RachaApiCollection.Name)]
public class BalancesEndpointsTests(RachaApiFactory factory)
{
    [Fact]
    public async Task GetBalances_ReturnsEachMembersBalanceAndSuggestedTransfers()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var bob = await factory.CreateUserAsync("Bob");
        var carol = await factory.CreateUserAsync("Carol");
        var group = await alice.CreateGroupAsync("Trip", bob, carol);
        await alice.CreateExpenseAsync(group.Id, "Dinner", 90m, alice.Id);

        var balances = (await carol.Client.GetFromJsonAsync<GroupBalancesDto>($"/api/groups/{group.Id}/balances"))!;

        balances.Balances.Should().BeEquivalentTo(new[]
        {
            new MemberBalanceDto(alice.Id, "Alice", 60m),
            new MemberBalanceDto(bob.Id, "Bob", -30m),
            new MemberBalanceDto(carol.Id, "Carol", -30m),
        });
        balances.SuggestedTransfers.Should().BeEquivalentTo(new[]
        {
            new TransferDto(bob.Id, "Bob", alice.Id, "Alice", 30m),
            new TransferDto(carol.Id, "Carol", alice.Id, "Alice", 30m),
        });
    }

    [Fact]
    public async Task GetBalances_AsNonMember_Returns404()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var outsider = await factory.CreateUserAsync("Outsider");
        var group = await alice.CreateGroupAsync("Trip");

        var response = await outsider.Client.GetAsync($"/api/groups/{group.Id}/balances");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
