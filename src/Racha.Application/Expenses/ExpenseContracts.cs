using Racha.Domain.Expenses;

namespace Racha.Application.Expenses;

/// <summary>Used for both creating and updating an expense.</summary>
public sealed record ExpenseRequest(string Description, decimal Amount, DateOnly Date, Guid PaidByUserId);

public sealed record ExpenseShareDto(Guid UserId, decimal Amount);

public sealed record ExpenseDto(
    Guid Id,
    Guid GroupId,
    string Description,
    decimal Amount,
    DateOnly Date,
    Guid PaidByUserId,
    IReadOnlyList<ExpenseShareDto> Shares)
{
    public static ExpenseDto From(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);

        return new ExpenseDto(
            expense.Id,
            expense.GroupId,
            expense.Description,
            expense.Amount,
            expense.Date,
            expense.PaidByUserId,
            expense.Shares
                .OrderBy(s => s.UserId)
                .Select(s => new ExpenseShareDto(s.UserId, s.Amount))
                .ToList());
    }
}
