using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Racha.Domain.Users;
using Racha.Infrastructure.Security;

namespace Racha.Infrastructure.Tests.Security;

public class TokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly JwtOptions JwtSettings = new()
    {
        Issuer = "racha-auth",
        Audience = "racha-api",
        SigningKey = "test-signing-key-that-is-at-least-32-characters-long",
        ExpirationMinutes = 60,
    };

    private readonly User _alice = User.Create("Alice", "alice@example.com", "hash");

    private static TokenService CreateService(JwtOptions? options = null) =>
        new(Options.Create(options ?? JwtSettings), new FixedTimeProvider(Now));

    [Fact]
    public void CreateToken_ContainsTheUserClaimsIssuerAndAudience()
    {
        var token = CreateService().CreateToken(_alice);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.Token);
        jwt.Subject.Should().Be(_alice.Id.ToString());
        jwt.GetClaim("email").Value.Should().Be("alice@example.com");
        jwt.GetClaim("name").Value.Should().Be("Alice");
        jwt.Issuer.Should().Be("racha-auth");
        jwt.Audiences.Should().ContainSingle().Which.Should().Be("racha-api");
    }

    [Fact]
    public void CreateToken_ExpiresAfterTheConfiguredMinutes()
    {
        var token = CreateService().CreateToken(_alice);

        token.ExpiresAt.Should().Be(Now.AddMinutes(60));
        new JsonWebTokenHandler().ReadJsonWebToken(token.Token).ValidTo
            .Should().Be(Now.AddMinutes(60).UtcDateTime);
    }

    [Fact]
    public async Task CreateToken_IsSignedWithTheConfiguredKey()
    {
        var token = CreateService().CreateToken(_alice);

        var result = await ValidateAsync(token.Token, JwtSettings.SigningKey);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateToken_FailsValidationWithADifferentKey()
    {
        var token = CreateService().CreateToken(_alice);

        var result = await ValidateAsync(token.Token, "another-signing-key-that-is-also-32-characters-long");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithSigningKeyShorterThan256Bits_Throws()
    {
        var weak = new JwtOptions
        {
            Issuer = JwtSettings.Issuer,
            Audience = JwtSettings.Audience,
            SigningKey = "too-short",
            ExpirationMinutes = 60,
        };

        var act = () => CreateService(weak);

        act.Should().Throw<InvalidOperationException>().WithMessage("*signing key*");
    }

    private static Task<TokenValidationResult> ValidateAsync(string token, string signingKey) =>
        new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = JwtSettings.Issuer,
            ValidAudience = JwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = false, // the token was issued at a fixed time in the past
        });

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
