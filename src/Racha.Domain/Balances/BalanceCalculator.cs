using Racha.Domain.Expenses;

namespace Racha.Domain.Balances;

/// <summary>Net balance of a member: positive means they are owed money, negative means they owe.</summary>
public sealed record MemberBalance(Guid UserId, decimal Amount);

public static class BalanceCalculator
{
    /// <summary>Computes each member's net balance (amount paid minus amount owed), ordered by user id.</summary>
    public static IReadOnlyList<MemberBalance> Calculate(IEnumerable<Guid> memberIds, IEnumerable<Expense> expenses)
    {
        ArgumentNullException.ThrowIfNull(memberIds);
        ArgumentNullException.ThrowIfNull(expenses);

        // Every member appears, even with a zero balance.
        var balances = memberIds.Distinct().ToDictionary(id => id, _ => 0m);

        foreach (var expense in expenses)
        {
            balances[expense.PaidByUserId] = balances.GetValueOrDefault(expense.PaidByUserId) + expense.Amount;

            foreach (var share in expense.Shares)
            {
                balances[share.UserId] = balances.GetValueOrDefault(share.UserId) - share.Amount;
            }
        }

        return balances
            .OrderBy(entry => entry.Key)
            .Select(entry => new MemberBalance(entry.Key, entry.Value))
            .ToList();
    }
}
