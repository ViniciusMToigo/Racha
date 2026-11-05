using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Racha.Application.Expenses;
using static Racha.Api.Tests.ApiTestHelpers;

namespace Racha.Api.Tests;

[Collection(RachaApiCollection.Name)]
public class ExpensesEndpointsTests(RachaApiFactory factory)
{
    [Fact]
    public async Task Expense_FullLifecycle_CreateReadUpdateDelete()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var bob = await factory.CreateUserAsync("Bob");
        var group = await alice.CreateGroupAsync("Trip", bob);

        // Create
        var created = await alice.Client.PostAsJsonAsync(
            $"/api/groups/{group.Id}/expenses", new ExpenseRequest("Dinner", 90m, Today, alice.Id));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var expense = (await created.Content.ReadFromJsonAsync<ExpenseDto>())!;
        created.Headers.Location!.ToString().Should().EndWith($"/api/expenses/{expense.Id}");
        expense.Shares.Should().HaveCount(2).And.OnlyContain(s => s.Amount == 45m);

        // Read (list and single), by the other member
        var list = (await bob.Client.GetFromJsonAsync<List<ExpenseDto>>($"/api/groups/{group.Id}/expenses"))!;
        list.Select(e => e.Id).Should().Contain(expense.Id);
        var single = (await bob.Client.GetFromJsonAsync<ExpenseDto>($"/api/expenses/{expense.Id}"))!;
        single.Description.Should().Be("Dinner");

        // Update
        var updated = await bob.Client.PutAsJsonAsync(
            $"/api/expenses/{expense.Id}", new ExpenseRequest("Dinner and drinks", 120m, Today, bob.Id));
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterUpdate = (await updated.Content.ReadFromJsonAsync<ExpenseDto>())!;
        afterUpdate.Amount.Should().Be(120m);
        afterUpdate.PaidByUserId.Should().Be(bob.Id);
        afterUpdate.Shares.Should().OnlyContain(s => s.Amount == 60m);

        // Delete
        var deleted = await alice.Client.DeleteAsync($"/api/expenses/{expense.Id}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await alice.Client.GetAsync($"/api/expenses/{expense.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateExpense_WithInvalidAmount_Returns400WithAmountError()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var group = await alice.CreateGroupAsync("Trip");

        var response = await alice.Client.PostAsJsonAsync(
            $"/api/groups/{group.Id}/expenses", new ExpenseRequest("Dinner", 0m, Today, alice.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Should().ContainKey("amount");
    }

    [Fact]
    public async Task CreateExpense_WithPayerOutsideTheGroup_Returns400BusinessRuleProblem()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var outsider = await factory.CreateUserAsync("Outsider");
        var group = await alice.CreateGroupAsync("Trip");

        var response = await alice.Client.PostAsJsonAsync(
            $"/api/groups/{group.Id}/expenses", new ExpenseRequest("Dinner", 90m, Today, outsider.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Contain("member");
    }

    [Fact]
    public async Task GetExpense_AsNonMember_Returns404()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var outsider = await factory.CreateUserAsync("Outsider");
        var group = await alice.CreateGroupAsync("Trip");
        var expense = await alice.CreateExpenseAsync(group.Id, "Dinner", 50m, alice.Id);

        var response = await outsider.Client.GetAsync($"/api/expenses/{expense.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
