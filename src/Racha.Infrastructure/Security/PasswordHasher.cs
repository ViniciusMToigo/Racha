using System.Globalization;
using System.Security.Cryptography;
using Racha.Application.Abstractions.Security;

namespace Racha.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 password hashing with a random salt per password.
/// Stored format: <c>v1.{iterations}.{base64 salt}.{base64 hash}</c>.
/// </summary>
/// <remarks>
/// Storing the version and iteration count with each hash lets us raise the cost in the future
/// without invalidating existing passwords: old hashes keep verifying with their own parameters.
/// </remarks>
public sealed class PasswordHasher : IPasswordHasher
{
    private const string Version = "v1";

    /// <summary>OWASP Password Storage Cheat Sheet recommendation for PBKDF2-HMAC-SHA256.</summary>
    private const int Iterations = 600_000;

    /// <summary>Rejects stored hashes that would make verification absurdly slow (denial of service).</summary>
    private const int MaxIterations = 10_000_000;

    private const int SaltSize = 16;
    private const int HashSize = 32;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return string.Join(
            '.',
            Version,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('.');
        if (parts.Length != 4
            || parts[0] != Version
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations is <= 0 or > MaxIterations)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

        // Constant-time comparison: the time taken doesn't reveal how many bytes matched.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
