using Racha.Application.Abstractions.Persistence;

namespace Racha.Application.Balances;

// Red phase only: the dependencies are not used yet. Removed when the service is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class BalanceService(
    IGroupRepository groups,
    IExpenseRepository expenses,
    IUserRepository users) : IBalanceService
{
    public Task<GroupBalancesDto> GetAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
