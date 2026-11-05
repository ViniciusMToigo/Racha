namespace Racha.Domain.Expenses;

public sealed record Share(Guid UserId, decimal Amount);

/// <summary>Splits an amount equally, distributing leftover cents deterministically.</summary>
public static class EqualSplitter
{
    /// <summary>
    /// Splits <paramref name="amount"/> among the participants. Shares always add up to the exact amount;
    /// leftover cents go one each to the participants with the lowest ids.
    /// </summary>
    public static IReadOnlyList<Share> Split(decimal amount, IEnumerable<Guid> participantIds) =>
        throw new NotImplementedException();
}
