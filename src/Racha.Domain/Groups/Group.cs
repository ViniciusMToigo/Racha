using Racha.Domain.Common;

namespace Racha.Domain.Groups;

public sealed class Group : Entity
{
    public const int MaxNameLength = 100;

    private readonly List<GroupMember> _members = [];

    private Group()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public Guid CreatedByUserId { get; private set; }

    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();

    /// <summary>Creates a group. The creator automatically becomes its first member.</summary>
    public static Group Create(string name, Guid createdByUserId)
    {
        var group = new Group
        {
            Name = Guard.RequiredText(name, MaxNameLength, "group name"),
            CreatedByUserId = Guard.RequiredId(createdByUserId, "group creator"),
        };

        group.AddMember(createdByUserId);
        return group;
    }

    public void Rename(string name) =>
        Name = Guard.RequiredText(name, MaxNameLength, "group name");

    public void AddMember(Guid userId)
    {
        Guard.RequiredId(userId, "member");

        if (IsMember(userId))
        {
            throw new DomainException("The user is already a member of this group.");
        }

        _members.Add(new GroupMember(Id, userId));
    }

    public bool IsMember(Guid userId) =>
        _members.Exists(m => m.UserId == userId);
}
