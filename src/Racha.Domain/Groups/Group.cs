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
    public static Group Create(string name, Guid createdByUserId) =>
        throw new NotImplementedException();

    public void Rename(string name) =>
        throw new NotImplementedException();

    public void AddMember(Guid userId) =>
        throw new NotImplementedException();

    public bool IsMember(Guid userId) =>
        throw new NotImplementedException();
}
