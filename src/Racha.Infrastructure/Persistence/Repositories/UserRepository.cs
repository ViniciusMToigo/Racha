using Microsoft.EntityFrameworkCore;
using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(RachaDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        // Translates to "WHERE id = ANY(@ids)": one round-trip, however many ids.
        return await db.Users.Where(u => idList.Contains(u.Id)).ToListAsync(cancellationToken);
    }

    public void Add(User user) => db.Users.Add(user);
}
