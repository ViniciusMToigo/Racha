using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Racha.Application.Groups;

namespace Racha.Api.Tests;

[Collection(RachaApiCollection.Name)]
public class GroupsEndpointsTests(RachaApiFactory factory)
{
    [Fact]
    public async Task ListGroups_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/groups");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateGroup_Returns201WithLocationAndTheCreatorAsMember()
    {
        var alice = await factory.CreateUserAsync("Alice");

        var response = await alice.Client.PostAsJsonAsync("/api/groups", new CreateGroupRequest("Trip to Floripa"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        response.Headers.Location!.ToString().Should().EndWith($"/api/groups/{group!.Id}");
        group.Name.Should().Be("Trip to Floripa");
        group.CreatedByUserId.Should().Be(alice.Id);
        group.Members.Should().ContainSingle().Which.Id.Should().Be(alice.Id);
    }

    [Fact]
    public async Task CreateGroup_WithBlankName_Returns400WithNameError()
    {
        var alice = await factory.CreateUserAsync("Alice");

        var response = await alice.Client.PostAsJsonAsync("/api/groups", new CreateGroupRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Should().ContainKey("name");
    }

    [Fact]
    public async Task ListGroups_ReturnsOnlyTheGroupsTheUserBelongsTo()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var bob = await factory.CreateUserAsync("Bob");
        var shared = await alice.CreateGroupAsync("Shared", bob);
        var bobsOwn = await bob.CreateGroupAsync("Bob only");

        var groups = (await alice.Client.GetFromJsonAsync<List<GroupSummaryDto>>("/api/groups"))!;

        groups.Select(g => g.Id).Should().Contain(shared.Id).And.NotContain(bobsOwn.Id);
        groups.Single(g => g.Id == shared.Id).MemberCount.Should().Be(2);
    }

    [Fact]
    public async Task GetGroup_AsNonMember_Returns404()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var outsider = await factory.CreateUserAsync("Outsider");
        var group = await alice.CreateGroupAsync("Private");

        var response = await outsider.Client.GetAsync($"/api/groups/{group.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddMember_ByEmail_LetsTheNewMemberSeeTheGroup()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var bob = await factory.CreateUserAsync("Bob");
        var group = await alice.CreateGroupAsync("House");

        var added = await alice.Client.PostAsJsonAsync($"/api/groups/{group.Id}/members", new AddMemberRequest(bob.Email));
        var seenByBob = await bob.Client.GetAsync($"/api/groups/{group.Id}");

        added.StatusCode.Should().Be(HttpStatusCode.OK);
        seenByBob.StatusCode.Should().Be(HttpStatusCode.OK);
        (await seenByBob.Content.ReadFromJsonAsync<GroupDto>())!.Members.Select(m => m.Id)
            .Should().BeEquivalentTo(new[] { alice.Id, bob.Id });
    }

    [Fact]
    public async Task AddMember_WithUnknownEmail_Returns404()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var group = await alice.CreateGroupAsync("House");

        var response = await alice.Client.PostAsJsonAsync(
            $"/api/groups/{group.Id}/members", new AddMemberRequest("ghost@test.dev"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateGroup_AsCreator_RenamesIt()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var group = await alice.CreateGroupAsync("House");

        var response = await alice.Client.PutAsJsonAsync($"/api/groups/{group.Id}", new UpdateGroupRequest("Beach house"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<GroupDto>())!.Name.Should().Be("Beach house");
    }

    [Fact]
    public async Task DeleteGroup_AsMemberWhoIsNotTheCreator_Returns403()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var bob = await factory.CreateUserAsync("Bob");
        var group = await alice.CreateGroupAsync("House", bob);

        var response = await bob.Client.DeleteAsync($"/api/groups/{group.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteGroup_AsCreator_Returns204AndTheGroupIsGone()
    {
        var alice = await factory.CreateUserAsync("Alice");
        var group = await alice.CreateGroupAsync("Temporary");

        var deleted = await alice.Client.DeleteAsync($"/api/groups/{group.Id}");
        var afterwards = await alice.Client.GetAsync($"/api/groups/{group.Id}");

        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        afterwards.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
