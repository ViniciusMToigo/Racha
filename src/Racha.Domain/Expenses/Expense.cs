using Racha.Domain.Common;
using Racha.Domain.Groups;

namespace Racha.Domain.Expenses;

public sealed class Expense : Entity
{
    public const int MaxDescriptionLength = 200;

    private readonly List<ExpenseShare> _shares = [];

    private Expense()
    {
    }

    public Guid GroupId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public DateOnly Date { get; private set; }

    public Guid PaidByUserId { get; private set; }

    public IReadOnlyCollection<ExpenseShare> Shares => _shares.AsReadOnly();

    /// <summary>Creates an expense split equally among all current members of the group.</summary>
    public static Expense Create(Group group, string description, decimal amount, DateOnly date, Guid paidByUserId) =>
        throw new NotImplementedException();

    /// <summary>Updates the expense and re-splits it among the current members of the group.</summary>
    public void Update(Group group, string description, decimal amount, DateOnly date, Guid paidByUserId) =>
        throw new NotImplementedException();
}
