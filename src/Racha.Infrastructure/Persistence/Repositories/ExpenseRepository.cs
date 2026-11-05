using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Expenses;

namespace Racha.Infrastructure.Persistence.Repositories;

// Red phase only: the dependencies are not used yet. Removed when the repository is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class ExpenseRepository(RachaDbContext db) : IExpenseRepository
{
    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Expense>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public void Add(Expense expense) =>
        throw new NotImplementedException();

    public void Remove(Expense expense) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
