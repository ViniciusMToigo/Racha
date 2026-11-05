using FluentValidation;
using Racha.Application.Abstractions.Persistence;

namespace Racha.Application.Groups;

// Red phase only: the dependencies are not used yet. Removed when the service is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class GroupService(
    IGroupRepository groups,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IValidator<CreateGroupRequest> createValidator,
    IValidator<UpdateGroupRequest> updateValidator,
    IValidator<AddMemberRequest> addMemberValidator) : IGroupService
{
    public Task<GroupDto> CreateAsync(Guid currentUserId, CreateGroupRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<GroupSummaryDto>> ListMineAsync(Guid currentUserId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<GroupDto> GetAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<GroupDto> UpdateAsync(Guid currentUserId, Guid groupId, UpdateGroupRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task DeleteAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<GroupDto> AddMemberAsync(Guid currentUserId, Guid groupId, AddMemberRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
