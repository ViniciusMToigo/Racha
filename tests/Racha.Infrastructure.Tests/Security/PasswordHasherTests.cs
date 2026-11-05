using FluentAssertions;
using Racha.Infrastructure.Security;

namespace Racha.Infrastructure.Tests.Security;

public class PasswordHasherTests
{
    private const string Password = "Passw0rd!";
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsVersionedFormatThatDoesNotContainThePassword()
    {
        var hash = _hasher.Hash(Password);

        hash.Should().StartWith("v1.").And.NotContain(Password);
        hash.Split('.').Should().HaveCount(4);
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashesBecauseOfTheSalt()
    {
        _hasher.Hash(Password).Should().NotBe(_hasher.Hash(Password));
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash(Password);

        _hasher.Verify(Password, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash(Password);

        _hasher.Verify("passw0rd!", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("v2.600000.c2FsdA==.aGFzaA==")]   // unknown version
    [InlineData("v1.lots.c2FsdA==.aGFzaA==")]     // iterations not a number
    [InlineData("v1.600000.!!!.###")]             // not base64
    public void Verify_WithMalformedHash_ReturnsFalseInsteadOfThrowing(string storedHash)
    {
        _hasher.Verify(Password, storedHash).Should().BeFalse();
    }

    [Fact]
    public void Hash_WithEmptyPassword_Throws()
    {
        var act = () => _hasher.Hash("");

        act.Should().Throw<ArgumentException>();
    }
}
