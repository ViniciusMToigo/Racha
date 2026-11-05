namespace Racha.Application.Expenses;

/// <summary>Any member of a group can create, read, update and delete its expenses.</summary>
public interface IExpenseService
{
    Task<ExpenseDto> CreateAsync(Guid currentUserId, Guid groupId, ExpenseRequest request, CancellationToken cancellationToken);

    /// <summary>Returns the group's expenses, newest first.</summary>
    Task<IReadOnlyList<ExpenseDto>> ListAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken);

    Task<ExpenseDto> GetAsync(Guid currentUserId, Guid expenseId, CancellationToken cancellationToken);

    Task<ExpenseDto> UpdateAsync(Guid currentUserId, Guid expenseId, ExpenseRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid currentUserId, Guid expenseId, CancellationToken cancellationToken);
}
