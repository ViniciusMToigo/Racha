using Racha.Domain.Groups;

namespace Racha.Application.Abstractions.Persistence;

public interface IGroupRepository
{
    /// <summary>Returns the group with its members loaded, or null.</summary>
    Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns every group the user is a member of, with members loaded.</summary>
    Task<IReadOnlyList<Group>> GetForUserAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Group group);

    void Remove(Group group);
}
