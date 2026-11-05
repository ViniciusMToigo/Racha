namespace Racha.Application.Groups;

public interface IGroupService
{
    Task<GroupDto> CreateAsync(Guid currentUserId, CreateGroupRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<GroupSummaryDto>> ListMineAsync(Guid currentUserId, CancellationToken cancellationToken);

    Task<GroupDto> GetAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken);

    /// <summary>Only the group creator can rename it.</summary>
    Task<GroupDto> UpdateAsync(Guid currentUserId, Guid groupId, UpdateGroupRequest request, CancellationToken cancellationToken);

    /// <summary>Only the group creator can delete it.</summary>
    Task DeleteAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken);

    /// <summary>Any member can add another registered user by e-mail.</summary>
    Task<GroupDto> AddMemberAsync(Guid currentUserId, Guid groupId, AddMemberRequest request, CancellationToken cancellationToken);
}
