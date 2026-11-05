namespace Racha.Application.Balances;

/// <summary>Positive amount: the member is owed money. Negative: the member owes money.</summary>
public sealed record MemberBalanceDto(Guid UserId, string Name, decimal Amount);

public sealed record TransferDto(Guid FromUserId, string FromName, Guid ToUserId, string ToName, decimal Amount);

public sealed record GroupBalancesDto(
    Guid GroupId,
    IReadOnlyList<MemberBalanceDto> Balances,
    IReadOnlyList<TransferDto> SuggestedTransfers);
