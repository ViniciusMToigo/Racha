using System.Globalization;
using FluentValidation.TestHelper;
using Racha.Application.Expenses;
using Racha.Domain.Expenses;

namespace Racha.Application.Tests.Expenses;

public class ExpenseRequestValidatorTests
{
    private readonly ExpenseRequestValidator _validator = new();

    private static ExpenseRequest ValidRequest() =>
        new("Dinner", 90m, new DateOnly(2026, 11, 4), Guid.NewGuid());

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        _validator.TestValidate(ValidRequest()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankDescription_HasError(string description)
    {
        _validator.TestValidate(ValidRequest() with { Description = description })
            .ShouldHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void DescriptionTooLong_HasError()
    {
        var request = ValidRequest() with { Description = new string('a', Expense.MaxDescriptionLength + 1) };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.Description);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("10.001")]
    public void InvalidAmount_HasError(string amount)
    {
        var request = ValidRequest() with { Amount = decimal.Parse(amount, CultureInfo.InvariantCulture) };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.Amount);
    }

    [Fact]
    public void MissingDate_HasError()
    {
        _validator.TestValidate(ValidRequest() with { Date = default })
            .ShouldHaveValidationErrorFor(r => r.Date);
    }

    [Fact]
    public void MissingPayer_HasError()
    {
        _validator.TestValidate(ValidRequest() with { PaidByUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(r => r.PaidByUserId);
    }
}
