using Microsoft.Extensions.Options;
using Racha.Application.Abstractions.Security;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Security;

// Red phase only: the dependencies are not used yet. Removed when the service is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    public AccessToken CreateToken(User user) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
