namespace Racha.Domain.Balances;

public sealed record Transfer(Guid FromUserId, Guid ToUserId, decimal Amount);

/// <summary>Turns net balances into a short list of payments that settles everyone.</summary>
public static class DebtSimplifier
{
    /// <summary>
    /// Returns at most (n - 1) transfers that bring every balance to zero.
    /// Balances must add up to zero.
    /// </summary>
    public static IReadOnlyList<Transfer> Simplify(IEnumerable<MemberBalance> balances) =>
        throw new NotImplementedException();
}
