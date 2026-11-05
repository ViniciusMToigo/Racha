using FluentValidation;

namespace Racha.Application.Auth;

/// <summary>Login only checks that fields are present and well-formed; password rules apply at registration.</summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(r => r.Password)
            .NotEmpty();
    }
}
