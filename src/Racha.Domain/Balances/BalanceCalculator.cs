using Racha.Domain.Expenses;

namespace Racha.Domain.Balances;

/// <summary>Net balance of a member: positive means they are owed money, negative means they owe.</summary>
public sealed record MemberBalance(Guid UserId, decimal Amount);

public static class BalanceCalculator
{
    /// <summary>Computes each member's net balance (amount paid minus amount owed), ordered by user id.</summary>
    public static IReadOnlyList<MemberBalance> Calculate(IEnumerable<Guid> memberIds, IEnumerable<Expense> expenses) =>
        throw new NotImplementedException();
}
