using Racha.Application.Abstractions.Persistence;
using Racha.Application.Groups;
using Racha.Domain.Balances;

namespace Racha.Application.Balances;

public sealed class BalanceService(
    IGroupRepository groups,
    IExpenseRepository expenses,
    IUserRepository users) : IBalanceService
{
    public async Task<GroupBalancesDto> GetAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken)
    {
        var group = await groups.GetForMemberAsync(groupId, currentUserId, cancellationToken);
        var groupExpenses = await expenses.GetByGroupAsync(groupId, cancellationToken);

        // The math lives in the domain; this service only loads data and adds names for display.
        var balances = BalanceCalculator.Calculate(group.Members.Select(m => m.UserId), groupExpenses);
        var transfers = DebtSimplifier.Simplify(balances);

        var people = await users.GetByIdsAsync(balances.Select(b => b.UserId), cancellationToken);
        var names = people.ToDictionary(u => u.Id, u => u.Name);
        string NameOf(Guid userId) => names.GetValueOrDefault(userId, "Unknown user");

        return new GroupBalancesDto(
            group.Id,
            balances.Select(b => new MemberBalanceDto(b.UserId, NameOf(b.UserId), b.Amount)).ToList(),
            transfers.Select(t => new TransferDto(t.FromUserId, NameOf(t.FromUserId), t.ToUserId, NameOf(t.ToUserId), t.Amount)).ToList());
    }
}
