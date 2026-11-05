using Microsoft.EntityFrameworkCore;
using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Groups;

namespace Racha.Infrastructure.Persistence.Repositories;

/// <summary>Groups are always loaded with their members: the domain rules (IsMember, AddMember) need them.</summary>
public sealed class GroupRepository(RachaDbContext db) : IGroupRepository
{
    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Group>> GetForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);

    public void Add(Group group) => db.Groups.Add(group);

    public void Remove(Group group) => db.Groups.Remove(group);
}
