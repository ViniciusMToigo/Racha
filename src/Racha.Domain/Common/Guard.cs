namespace Racha.Domain.Common;

/// <summary>Small reusable validation helpers shared by the domain entities.</summary>
internal static class Guard
{
    public const decimal MaxAmount = 1_000_000m;

    /// <summary>Ensures the text is not blank and fits the limit. Returns it trimmed.</summary>
    public static string RequiredText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"The {fieldName} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"The {fieldName} must have at most {maxLength} characters.");
        }

        return trimmed;
    }

    public static Guid RequiredId(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"The {fieldName} is required.");
        }

        return value;
    }

    /// <summary>Money amounts must be positive, have at most two decimal places and stay within a sane limit.</summary>
    public static decimal ValidAmount(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new DomainException("The amount must be greater than zero.");
        }

        if (amount > MaxAmount)
        {
            throw new DomainException($"The amount cannot exceed {MaxAmount}.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new DomainException("The amount cannot have more than two decimal places.");
        }

        return amount;
    }
}
