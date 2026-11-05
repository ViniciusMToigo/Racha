using FluentValidation.TestHelper;
using Racha.Application.Auth;
using Racha.Domain.Users;

namespace Racha.Application.Tests.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() => new("Alice", "alice@example.com", "Passw0rd!");

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        _validator.TestValidate(ValidRequest()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankName_HasError(string name)
    {
        var result = _validator.TestValidate(ValidRequest() with { Name = name });

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public void NameTooLong_HasError()
    {
        var request = ValidRequest() with { Name = new string('a', User.MaxNameLength + 1) };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("alice@")]
    public void InvalidEmail_HasError(string email)
    {
        var result = _validator.TestValidate(ValidRequest() with { Email = email });

        result.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short1A")]       // fewer than 8 characters
    [InlineData("lowercase1")]    // no upper-case letter
    [InlineData("UPPERCASE1")]    // no lower-case letter
    [InlineData("NoDigitsHere")]  // no digit
    public void WeakPassword_HasError(string password)
    {
        var result = _validator.TestValidate(ValidRequest() with { Password = password });

        result.ShouldHaveValidationErrorFor(r => r.Password);
    }
}
