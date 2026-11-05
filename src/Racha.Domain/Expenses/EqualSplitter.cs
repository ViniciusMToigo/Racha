using Racha.Domain.Common;

namespace Racha.Domain.Expenses;

public sealed record Share(Guid UserId, decimal Amount);

/// <summary>Splits an amount equally, distributing leftover cents deterministically.</summary>
public static class EqualSplitter
{
    /// <summary>
    /// Splits <paramref name="amount"/> among the participants. Shares always add up to the exact amount;
    /// leftover cents go one each to the participants with the lowest ids.
    /// </summary>
    public static IReadOnlyList<Share> Split(decimal amount, IEnumerable<Guid> participantIds)
    {
        ArgumentNullException.ThrowIfNull(participantIds);
        Guard.ValidAmount(amount);

        var participants = participantIds.Order().ToList();
        if (participants.Count == 0)
        {
            throw new DomainException("At least one participant is required to split an expense.");
        }

        if (participants.Distinct().Count() != participants.Count)
        {
            throw new DomainException("Each participant can only appear once in a split.");
        }

        // Work in whole cents to avoid any rounding drift.
        var totalCents = (long)(amount * 100m);
        var baseCents = totalCents / participants.Count;
        var leftoverCents = totalCents % participants.Count;

        return participants
            .Select((userId, index) =>
                new Share(userId, (baseCents + (index < leftoverCents ? 1 : 0)) / 100m))
            .ToList();
    }
}
