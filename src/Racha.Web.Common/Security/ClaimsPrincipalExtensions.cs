using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Racha.Web.Common.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the user id from the token's "sub" claim.</summary>
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("The access token does not identify a user.");
    }
}
