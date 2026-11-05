using Racha.Domain.Expenses;

namespace Racha.Application.Abstractions.Persistence;

public interface IExpenseRepository
{
    /// <summary>Returns the expense with its shares loaded, or null.</summary>
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns all expenses of a group with their shares loaded.</summary>
    Task<IReadOnlyList<Expense>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken);

    void Add(Expense expense);

    void Remove(Expense expense);
}
