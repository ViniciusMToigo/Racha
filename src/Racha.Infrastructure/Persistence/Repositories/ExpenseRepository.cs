using Microsoft.EntityFrameworkCore;
using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Expenses;

namespace Racha.Infrastructure.Persistence.Repositories;

/// <summary>Expenses are always loaded with their shares, since balances and updates depend on them.</summary>
public sealed class ExpenseRepository(RachaDbContext db) : IExpenseRepository
{
    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Expenses
            .Include(e => e.Shares)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Expense>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken) =>
        await db.Expenses
            .Include(e => e.Shares)
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

    public void Add(Expense expense) => db.Expenses.Add(expense);

    public void Remove(Expense expense) => db.Expenses.Remove(expense);
}
