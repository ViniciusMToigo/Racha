using Racha.Domain.Users;

namespace Racha.Application.Abstractions.Security;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Issues access tokens (JWT) for authenticated users.</summary>
public interface ITokenService
{
    AccessToken CreateToken(User user);
}
