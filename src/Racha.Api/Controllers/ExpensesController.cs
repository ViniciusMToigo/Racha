using Microsoft.AspNetCore.Mvc;
using Racha.Application.Expenses;
using Racha.Web.Common.Controllers;

namespace Racha.Api.Controllers;

/// <summary>
/// Expenses are created and listed under their group (/api/groups/{groupId}/expenses)
/// and addressed directly by id afterwards (/api/expenses/{expenseId}).
/// </summary>
[Route("api")]
public sealed class ExpensesController(IExpenseService expenses) : ApiControllerBase
{
    /// <summary>Lists a group's expenses, newest first.</summary>
    [HttpGet("groups/{groupId:guid}/expenses")]
    [ProducesResponseType<IReadOnlyList<ExpenseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> List(Guid groupId, CancellationToken cancellationToken) =>
        Ok(await expenses.ListAsync(CurrentUserId, groupId, cancellationToken));

    /// <summary>Records an expense, split equally among the group's current members.</summary>
    [HttpPost("groups/{groupId:guid}/expenses")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseDto>> Create(Guid groupId, ExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = await expenses.CreateAsync(CurrentUserId, groupId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { expenseId = expense.Id }, expense);
    }

    /// <summary>Returns an expense with each member's share.</summary>
    [HttpGet("expenses/{expenseId:guid}")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseDto>> Get(Guid expenseId, CancellationToken cancellationToken) =>
        Ok(await expenses.GetAsync(CurrentUserId, expenseId, cancellationToken));

    /// <summary>Updates an expense and re-splits it among the group's current members.</summary>
    [HttpPut("expenses/{expenseId:guid}")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseDto>> Update(Guid expenseId, ExpenseRequest request, CancellationToken cancellationToken) =>
        Ok(await expenses.UpdateAsync(CurrentUserId, expenseId, request, cancellationToken));

    /// <summary>Deletes an expense.</summary>
    [HttpDelete("expenses/{expenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid expenseId, CancellationToken cancellationToken)
    {
        await expenses.DeleteAsync(CurrentUserId, expenseId, cancellationToken);
        return NoContent();
    }
}
