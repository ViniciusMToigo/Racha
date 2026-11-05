using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Application.Groups;
using Racha.Domain.Common;
using Racha.Domain.Groups;
using Racha.Domain.Users;
using static Racha.Application.Tests.TestData;

namespace Racha.Application.Tests.Groups;

public class GroupServiceTests
{
    private readonly IGroupRepository _groups = Substitute.For<IGroupRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GroupService _sut;

    public GroupServiceTests()
    {
        _users.ResolvesKnownUsers();
        _sut = new GroupService(
            _groups,
            _users,
            _unitOfWork,
            new CreateGroupRequestValidator(),
            new UpdateGroupRequestValidator(),
            new AddMemberRequestValidator());
    }

    private Group StoredGroup(User creator, params User[] others)
    {
        var group = GroupOf(creator, others);
        _groups.GetByIdAsync(group.Id, Ct).Returns(group);
        return group;
    }

    [Fact]
    public async Task CreateAsync_CreatesGroupWithCurrentUserAsOnlyMember()
    {
        var result = await _sut.CreateAsync(Alice.Id, new CreateGroupRequest("Trip to Floripa"), Ct);

        result.Name.Should().Be("Trip to Floripa");
        result.CreatedByUserId.Should().Be(Alice.Id);
        result.Members.Should().ContainSingle().Which.Name.Should().Be("Alice");
        _groups.Received(1).Add(Arg.Is<Group>(g => g.Id == result.Id && g.IsMember(Alice.Id)));
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task CreateAsync_WithBlankName_ThrowsValidationAndSavesNothing()
    {
        var act = () => _sut.CreateAsync(Alice.Id, new CreateGroupRequest(""), Ct);

        await act.Should().ThrowAsync<ValidationException>();
        _groups.DidNotReceive().Add(Arg.Any<Group>());
    }

    [Fact]
    public async Task ListMineAsync_ReturnsSummariesOfTheUsersGroups()
    {
        var trip = GroupOf(Alice, Bob, Carol);
        var house = GroupOf(Bob, Alice);
        _groups.GetForUserAsync(Alice.Id, Ct).Returns(new[] { trip, house });

        var result = await _sut.ListMineAsync(Alice.Id, Ct);

        result.Should().BeEquivalentTo(new[]
        {
            new GroupSummaryDto(trip.Id, trip.Name, 3),
            new GroupSummaryDto(house.Id, house.Name, 2),
        });
    }

    [Fact]
    public async Task GetAsync_AsMember_ReturnsGroupWithMemberDetails()
    {
        var group = StoredGroup(Alice, Bob);

        var result = await _sut.GetAsync(Bob.Id, group.Id, Ct);

        result.Id.Should().Be(group.Id);
        result.Members.Select(m => m.Name).Should().BeEquivalentTo(new[] { "Alice", "Bob" });
    }

    [Fact]
    public async Task GetAsync_AsNonMember_ThrowsNotFound()
    {
        var group = StoredGroup(Alice, Bob);

        var act = () => _sut.GetAsync(Dave.Id, group.Id, Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAsync_UnknownGroup_ThrowsNotFound()
    {
        var act = () => _sut.GetAsync(Alice.Id, Guid.NewGuid(), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_AsCreator_RenamesAndSaves()
    {
        var group = StoredGroup(Alice, Bob);

        var result = await _sut.UpdateAsync(Alice.Id, group.Id, new UpdateGroupRequest("Beach house"), Ct);

        result.Name.Should().Be("Beach house");
        group.Name.Should().Be("Beach house");
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task UpdateAsync_AsMemberWhoIsNotCreator_ThrowsForbidden()
    {
        var group = StoredGroup(Alice, Bob);

        var act = () => _sut.UpdateAsync(Bob.Id, group.Id, new UpdateGroupRequest("Beach house"), Ct);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_AsCreator_RemovesAndSaves()
    {
        var group = StoredGroup(Alice, Bob);

        await _sut.DeleteAsync(Alice.Id, group.Id, Ct);

        _groups.Received(1).Remove(group);
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task DeleteAsync_AsMemberWhoIsNotCreator_ThrowsForbidden()
    {
        var group = StoredGroup(Alice, Bob);

        var act = () => _sut.DeleteAsync(Bob.Id, group.Id, Ct);

        await act.Should().ThrowAsync<ForbiddenException>();
        _groups.DidNotReceive().Remove(Arg.Any<Group>());
    }

    [Fact]
    public async Task AddMemberAsync_WithRegisteredEmail_AddsUserAndSaves()
    {
        var group = StoredGroup(Alice);
        _users.GetByEmailAsync("bob@example.com", Ct).Returns(Bob);

        var result = await _sut.AddMemberAsync(Alice.Id, group.Id, new AddMemberRequest(" Bob@Example.com "), Ct);

        result.Members.Select(m => m.Id).Should().Contain(Bob.Id);
        group.IsMember(Bob.Id).Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task AddMemberAsync_WithUnknownEmail_ThrowsNotFound()
    {
        var group = StoredGroup(Alice);
        _users.GetByEmailAsync("ghost@example.com", Ct).Returns((User?)null);

        var act = () => _sut.AddMemberAsync(Alice.Id, group.Id, new AddMemberRequest("ghost@example.com"), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddMemberAsync_WhenUserIsAlreadyMember_ThrowsDomainError()
    {
        var group = StoredGroup(Alice, Bob);
        _users.GetByEmailAsync("bob@example.com", Ct).Returns(Bob);

        var act = () => _sut.AddMemberAsync(Alice.Id, group.Id, new AddMemberRequest("bob@example.com"), Ct);

        await act.Should().ThrowAsync<DomainException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddMemberAsync_AsNonMember_ThrowsNotFound()
    {
        var group = StoredGroup(Alice);

        var act = () => _sut.AddMemberAsync(Dave.Id, group.Id, new AddMemberRequest("bob@example.com"), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
