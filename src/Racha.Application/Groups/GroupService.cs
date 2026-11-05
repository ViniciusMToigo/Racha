using FluentValidation;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Application.Users;
using Racha.Domain.Groups;
using Racha.Domain.Users;

namespace Racha.Application.Groups;

public sealed class GroupService(
    IGroupRepository groups,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IValidator<CreateGroupRequest> createValidator,
    IValidator<UpdateGroupRequest> updateValidator,
    IValidator<AddMemberRequest> addMemberValidator) : IGroupService
{
    public async Task<GroupDto> CreateAsync(Guid currentUserId, CreateGroupRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var group = Group.Create(request.Name, currentUserId);
        groups.Add(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(group, cancellationToken);
    }

    public async Task<IReadOnlyList<GroupSummaryDto>> ListMineAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        var myGroups = await groups.GetForUserAsync(currentUserId, cancellationToken);

        return myGroups
            .Select(g => new GroupSummaryDto(g.Id, g.Name, g.Members.Count))
            .ToList();
    }

    public async Task<GroupDto> GetAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken)
    {
        var group = await groups.GetForMemberAsync(groupId, currentUserId, cancellationToken);
        return await ToDtoAsync(group, cancellationToken);
    }

    public async Task<GroupDto> UpdateAsync(Guid currentUserId, Guid groupId, UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var group = await GetForCreatorAsync(groupId, currentUserId, cancellationToken);
        group.Rename(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(group, cancellationToken);
    }

    public async Task DeleteAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken)
    {
        var group = await GetForCreatorAsync(groupId, currentUserId, cancellationToken);
        groups.Remove(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<GroupDto> AddMemberAsync(Guid currentUserId, Guid groupId, AddMemberRequest request, CancellationToken cancellationToken)
    {
        await addMemberValidator.ValidateAndThrowAsync(request, cancellationToken);

        var group = await groups.GetForMemberAsync(groupId, currentUserId, cancellationToken);

        var email = User.NormalizeEmail(request.Email);
        var user = await users.GetByEmailAsync(email, cancellationToken)
            ?? throw new NotFoundException("User", email);

        group.AddMember(user.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(group, cancellationToken);
    }

    /// <summary>Members who are not the creator can see the group (403), outsiders can't (404).</summary>
    private async Task<Group> GetForCreatorAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var group = await groups.GetForMemberAsync(groupId, userId, cancellationToken);

        if (group.CreatedByUserId != userId)
        {
            throw new ForbiddenException("Only the group creator can perform this action.");
        }

        return group;
    }

    private async Task<GroupDto> ToDtoAsync(Group group, CancellationToken cancellationToken)
    {
        var members = await users.GetByIdsAsync(group.Members.Select(m => m.UserId), cancellationToken);

        return new GroupDto(
            group.Id,
            group.Name,
            group.CreatedByUserId,
            members.OrderBy(u => u.Name, StringComparer.OrdinalIgnoreCase).Select(UserDto.From).ToList());
    }
}
