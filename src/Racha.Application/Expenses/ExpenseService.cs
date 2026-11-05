using FluentValidation;
using Racha.Application.Abstractions.Persistence;

namespace Racha.Application.Expenses;

// Red phase only: the dependencies are not used yet. Removed when the service is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class ExpenseService(
    IExpenseRepository expenses,
    IGroupRepository groups,
    IUnitOfWork unitOfWork,
    IValidator<ExpenseRequest> validator) : IExpenseService
{
    public Task<ExpenseDto> CreateAsync(Guid currentUserId, Guid groupId, ExpenseRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<ExpenseDto>> ListAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<ExpenseDto> GetAsync(Guid currentUserId, Guid expenseId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<ExpenseDto> UpdateAsync(Guid currentUserId, Guid expenseId, ExpenseRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task DeleteAsync(Guid currentUserId, Guid expenseId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
