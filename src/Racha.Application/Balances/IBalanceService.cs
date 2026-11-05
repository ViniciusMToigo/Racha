namespace Racha.Application.Balances;

public interface IBalanceService
{
    /// <summary>Returns each member's balance and the payments that would settle the group.</summary>
    Task<GroupBalancesDto> GetAsync(Guid currentUserId, Guid groupId, CancellationToken cancellationToken);
}
