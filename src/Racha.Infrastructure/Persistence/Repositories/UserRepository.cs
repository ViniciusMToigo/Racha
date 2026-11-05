using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence.Repositories;

// Red phase only: the dependencies are not used yet. Removed when the repository is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class UserRepository(RachaDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public void Add(User user) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
