namespace Racha.Domain.Expenses;

/// <summary>The part of an expense owed by one member. Identified by (ExpenseId, UserId).</summary>
public sealed class ExpenseShare
{
    private ExpenseShare()
    {
    }

    internal ExpenseShare(Guid expenseId, Guid userId, decimal amount)
    {
        ExpenseId = expenseId;
        UserId = userId;
        Amount = amount;
    }

    public Guid ExpenseId { get; private set; }

    public Guid UserId { get; private set; }

    public decimal Amount { get; private set; }

    internal void ChangeAmount(decimal amount) => Amount = amount;
}
