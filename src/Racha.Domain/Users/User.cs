using Racha.Domain.Common;

namespace Racha.Domain.Users;

public sealed class User : Entity
{
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 254;

    private User()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Always stored trimmed and lower-case so lookups are case-insensitive.</summary>
    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public static User Create(string name, string email, string passwordHash) =>
        new()
        {
            Name = Guard.RequiredText(name, MaxNameLength, "name"),
            Email = NormalizeEmail(email),
            PasswordHash = Guard.RequiredText(passwordHash, int.MaxValue, "password hash"),
        };

    /// <summary>
    /// Trims and lower-cases the e-mail and runs a structural check (one "@", no spaces, text on both sides).
    /// Full format validation belongs to the Application layer's input validators.
    /// </summary>
    public static string NormalizeEmail(string email)
    {
        var normalized = Guard.RequiredText(email, MaxEmailLength, "email").ToLowerInvariant();

        var at = normalized.IndexOf('@', StringComparison.Ordinal);
        var isValid = at > 0
            && at == normalized.LastIndexOf('@')
            && at < normalized.Length - 1
            && !normalized.Contains(' ', StringComparison.Ordinal);

        if (!isValid)
        {
            throw new DomainException("The email is not a valid address.");
        }

        return normalized;
    }
}
