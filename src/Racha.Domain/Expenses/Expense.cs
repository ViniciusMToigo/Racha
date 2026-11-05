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
    public static Expense Create(Group group, string description, decimal amount, DateOnly date, Guid paidByUserId)
    {
        ArgumentNullException.ThrowIfNull(group);

        var expense = new Expense { GroupId = group.Id };
        expense.Apply(group, description, amount, date, paidByUserId);
        return expense;
    }

    /// <summary>Updates the expense and re-splits it among the current members of the group.</summary>
    public void Update(Group group, string description, decimal amount, DateOnly date, Guid paidByUserId)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (group.Id != GroupId)
        {
            throw new DomainException("An expense cannot be moved to another group.");
        }

        Apply(group, description, amount, date, paidByUserId);
    }

    private void Apply(Group group, string description, decimal amount, DateOnly date, Guid paidByUserId)
    {
        // Validate everything before mutating, so a failed update leaves the expense untouched.
        var validDescription = Guard.RequiredText(description, MaxDescriptionLength, "description");

        if (!group.IsMember(paidByUserId))
        {
            throw new DomainException("The payer must be a member of the group.");
        }

        var newShares = EqualSplitter.Split(amount, group.Members.Select(m => m.UserId));

        Description = validDescription;
        Amount = amount;
        Date = date;
        PaidByUserId = paidByUserId;
        ReconcileShares(newShares);
    }

    /// <summary>
    /// Updates existing shares in place instead of clearing and re-adding them,
    /// so EF Core tracks them as updates rather than delete + insert of the same key.
    /// </summary>
    private void ReconcileShares(IReadOnlyList<Share> newShares)
    {
        _shares.RemoveAll(existing => !newShares.Any(n => n.UserId == existing.UserId));

        foreach (var share in newShares)
        {
            var existing = _shares.Find(s => s.UserId == share.UserId);
            if (existing is null)
            {
                _shares.Add(new ExpenseShare(Id, share.UserId, share.Amount));
            }
            else
            {
                existing.ChangeAmount(share.Amount);
            }
        }
    }
}
