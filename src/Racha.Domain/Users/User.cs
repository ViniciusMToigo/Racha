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
        throw new NotImplementedException();
}
