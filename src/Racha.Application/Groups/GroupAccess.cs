using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Domain.Groups;

namespace Racha.Application.Groups;

internal static class GroupAccess
{
    /// <summary>
    /// Loads a group the user belongs to. A missing group and a group the user is not in
    /// produce the same <see cref="NotFoundException"/>, so outsiders can't probe which ids exist.
    /// </summary>
    public static async Task<Group> GetForMemberAsync(
        this IGroupRepository groups, Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var group = await groups.GetByIdAsync(groupId, cancellationToken);

        if (group is null || !group.IsMember(userId))
        {
            throw new NotFoundException("Group", groupId);
        }

        return group;
    }
}
