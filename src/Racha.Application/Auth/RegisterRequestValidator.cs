using FluentValidation;

namespace Racha.Application.Auth;

/// <summary>Input validation: returns friendly, per-field errors before any business rule runs.</summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
}
