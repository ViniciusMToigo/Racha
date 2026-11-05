using Racha.Domain.Common;

namespace Racha.Domain.Balances;

public sealed record Transfer(Guid FromUserId, Guid ToUserId, decimal Amount);

/// <summary>Turns net balances into a short list of payments that settles everyone.</summary>
public static class DebtSimplifier
{
    /// <summary>
    /// Returns at most (n - 1) transfers that bring every balance to zero.
    /// Balances must add up to zero.
    /// </summary>
    /// <remarks>
    /// Greedy strategy: the largest debtor pays the largest creditor, repeatedly.
    /// Each step settles at least one person, which guarantees the (n - 1) bound.
    /// Finding the true minimum number of transfers is NP-hard and not worth it for small groups.
    /// </remarks>
    public static IReadOnlyList<Transfer> Simplify(IEnumerable<MemberBalance> balances)
    {
        ArgumentNullException.ThrowIfNull(balances);

        var all = balances.ToList();
        if (all.Sum(b => b.Amount) != 0m)
        {
            throw new DomainException("Balances must add up to zero.");
        }

        var debtors = all.Where(b => b.Amount < 0m).OrderBy(b => b.Amount).ThenBy(b => b.UserId).ToList();
        var creditors = all.Where(b => b.Amount > 0m).OrderByDescending(b => b.Amount).ThenBy(b => b.UserId).ToList();

        var stillOwes = debtors.Select(d => -d.Amount).ToArray();
        var stillDue = creditors.Select(c => c.Amount).ToArray();

        var transfers = new List<Transfer>();
        int debtor = 0, creditor = 0;

        while (debtor < debtors.Count && creditor < creditors.Count)
        {
            var amount = Math.Min(stillOwes[debtor], stillDue[creditor]);
            transfers.Add(new Transfer(debtors[debtor].UserId, creditors[creditor].UserId, amount));

            stillOwes[debtor] -= amount;
            stillDue[creditor] -= amount;

            if (stillOwes[debtor] == 0m)
            {
                debtor++;
            }

            if (stillDue[creditor] == 0m)
            {
                creditor++;
            }
        }

        return transfers;
    }
}
