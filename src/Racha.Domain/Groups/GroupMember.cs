namespace Racha.Domain.Groups;

/// <summary>Links a user to a group. Identified by (GroupId, UserId).</summary>
public sealed class GroupMember
{
    private GroupMember()
    {
    }

    internal GroupMember(Guid groupId, Guid userId)
    {
        GroupId = groupId;
        UserId = userId;
    }

    public Guid GroupId { get; private set; }

    public Guid UserId { get; private set; }
}
