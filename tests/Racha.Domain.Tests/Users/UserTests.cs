using FluentAssertions;
using Racha.Domain.Common;
using Racha.Domain.Users;

namespace Racha.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_TrimsNameAndNormalizesEmail()
    {
        var user = User.Create("  Alice  ", "  Alice@Example.COM ", "hashed-password");

        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("Alice");
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("hashed-password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        var act = () => User.Create(name, "alice@example.com", "hash");

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithNameTooLong_Throws()
    {
        var name = new string('a', User.MaxNameLength + 1);

        var act = () => User.Create(name, "alice@example.com", "hash");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("alice@")]
    [InlineData("alice@@example.com")]
    [InlineData("alice smith@example.com")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        var act = () => User.Create("Alice", email, "hash");

        act.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankPasswordHash_Throws(string passwordHash)
    {
        var act = () => User.Create("Alice", "alice@example.com", passwordHash);

        act.Should().Throw<DomainException>();
    }
}
