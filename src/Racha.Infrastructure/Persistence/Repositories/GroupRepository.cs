using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Groups;

namespace Racha.Infrastructure.Persistence.Repositories;

// Red phase only: the dependencies are not used yet. Removed when the repository is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class GroupRepository(RachaDbContext db) : IGroupRepository
{
    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Group>> GetForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public void Add(Group group) =>
        throw new NotImplementedException();

    public void Remove(Group group) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
