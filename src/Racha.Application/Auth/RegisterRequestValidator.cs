using FluentValidation;
using Racha.Domain.Users;

namespace Racha.Application.Auth;

/// <summary>Input validation: returns friendly, per-field errors before any business rule runs.</summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    public RegisterRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(User.MaxNameLength);

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.MaxEmailLength);

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(MinPasswordLength)
            .MaximumLength(MaxPasswordLength)
            .Matches("[A-Z]").WithMessage("Password must contain at least one upper-case letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lower-case letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
