using FluentValidation;
using Racha.Domain.Expenses;

namespace Racha.Application.Expenses;

/// <summary>
/// Friendly per-field errors for the API. The domain enforces the same invariants on its own,
/// so an entity can never end up invalid even if a caller skips this validator.
/// </summary>
public sealed class ExpenseRequestValidator : AbstractValidator<ExpenseRequest>
{
    public ExpenseRequestValidator()
    {
        RuleFor(r => r.Description)
            .NotEmpty()
            .MaximumLength(Expense.MaxDescriptionLength);

        RuleFor(r => r.Amount)
            .GreaterThan(0m)
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Amount cannot have more than two decimal places.");

        RuleFor(r => r.Date)
            .NotEmpty();

        RuleFor(r => r.PaidByUserId)
            .NotEmpty();
    }
}
