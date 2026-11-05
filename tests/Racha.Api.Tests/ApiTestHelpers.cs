using System.Net.Http.Json;
using Racha.Application.Expenses;
using Racha.Application.Groups;

namespace Racha.Api.Tests;

internal static class ApiTestHelpers
{
    public static readonly DateOnly Today = new(2026, 11, 4);

    /// <summary>Creates a group as <paramref name="owner"/> and adds the other users as members, through the API.</summary>
    public static async Task<GroupDto> CreateGroupAsync(this ApiUser owner, string name, params ApiUser[] members)
    {
        var response = await owner.Client.PostAsJsonAsync("/api/groups", new CreateGroupRequest(name));
        response.EnsureSuccessStatusCode();
        var group = (await response.Content.ReadFromJsonAsync<GroupDto>())!;

        foreach (var member in members)
        {
            var added = await owner.Client.PostAsJsonAsync($"/api/groups/{group.Id}/members", new AddMemberRequest(member.Email));
            added.EnsureSuccessStatusCode();
            group = (await added.Content.ReadFromJsonAsync<GroupDto>())!;
        }

        return group;
    }

    public static async Task<ExpenseDto> CreateExpenseAsync(
        this ApiUser user, Guid groupId, string description, decimal amount, Guid paidByUserId)
    {
        var response = await user.Client.PostAsJsonAsync(
            $"/api/groups/{groupId}/expenses", new ExpenseRequest(description, amount, Today, paidByUserId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>())!;
    }
}
