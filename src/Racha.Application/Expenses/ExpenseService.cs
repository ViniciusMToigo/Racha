using FluentValidation;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Application.Groups;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;

namespace Racha.Application.Expenses;

public sealed class ExpenseService(
    IExpenseRepository expenses,
    IGroupRepository groups,
    IUnitOfWork unitOfWork,
    IValidator<ExpenseRequest> validator) : IExpenseService
{
    public async Task<ExpenseDto> CreateAsync(Guid currentUserId, Guid groupId, ExpenseRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var group = await groups.GetForMemberAsync(groupId, currentUserId, cancellationToken);
        var expense = Expense.Create(group, request.Description, request.Amount, request.Date, request.PaidByUserId);

        expenses.Add(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseDto.From(expense);
    }

    public async Task<IReadOnlyList<ExpenseDto>> ListAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken)
    {
        await groups.GetForMemberAsync(groupId, currentUserId, cancellationToken);

        var groupExpenses = await expenses.GetByGroupAsync(groupId, cancellationToken);

        return groupExpenses
            .OrderByDescending(e => e.Date)
            .Select(ExpenseDto.From)
            .ToList();
    }

    public async Task<ExpenseDto> GetAsync(Guid currentUserId, Guid expenseId, CancellationToken cancellationToken)
    {
        var (expense, _) = await GetForMemberAsync(expenseId, currentUserId, cancellationToken);
        return ExpenseDto.From(expense);
    }

    public async Task<ExpenseDto> UpdateAsync(Guid currentUserId, Guid expenseId, ExpenseRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var (expense, group) = await GetForMemberAsync(expenseId, currentUserId, cancellationToken);
        expense.Update(group, request.Description, request.Amount, request.Date, request.PaidByUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseDto.From(expense);
    }

    public async Task DeleteAsync(Guid currentUserId, Guid expenseId, CancellationToken cancellationToken)
    {
        var (expense, _) = await GetForMemberAsync(expenseId, currentUserId, cancellationToken);

        expenses.Remove(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads an expense together with its group, but only if the user is a member of that group.
    /// Otherwise it reports the expense as not found, without revealing that it exists.
    /// </summary>
    private async Task<(Expense Expense, Group Group)> GetForMemberAsync(
        Guid expenseId, Guid userId, CancellationToken cancellationToken)
    {
        var expense = await expenses.GetByIdAsync(expenseId, cancellationToken);
        var group = expense is null ? null : await groups.GetByIdAsync(expense.GroupId, cancellationToken);

        if (expense is null || group is null || !group.IsMember(userId))
        {
            throw new NotFoundException("Expense", expenseId);
        }

        return (expense, group);
    }
}
