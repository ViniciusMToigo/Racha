using Racha.Application.Abstractions.Security;

namespace Racha.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 password hashing with a random salt per password.
/// Stored format: <c>v1.{iterations}.{base64 salt}.{base64 hash}</c>.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        throw new NotImplementedException();

    public bool Verify(string password, string passwordHash) =>
        throw new NotImplementedException();
}
