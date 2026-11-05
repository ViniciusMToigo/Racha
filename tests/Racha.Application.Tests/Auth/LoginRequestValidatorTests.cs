using FluentValidation.TestHelper;
using Racha.Application.Auth;

namespace Racha.Application.Tests.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        _validator.TestValidate(new LoginRequest("alice@example.com", "anything"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_HasError(string email)
    {
        _validator.TestValidate(new LoginRequest(email, "anything"))
            .ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Fact]
    public void BlankPassword_HasError()
    {
        // Login only checks presence: password rules apply at registration, not here.
        _validator.TestValidate(new LoginRequest("alice@example.com", ""))
            .ShouldHaveValidationErrorFor(r => r.Password);
    }
}
