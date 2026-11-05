using Microsoft.AspNetCore.Mvc;
using Racha.Application.Balances;
using Racha.Web.Common.Controllers;

namespace Racha.Api.Controllers;

/// <summary>Who owes whom in a group.</summary>
[Route("api/groups/{groupId:guid}/balances")]
public sealed class BalancesController(IBalanceService balances) : ApiControllerBase
{
    /// <summary>Returns each member's net balance and the payments that would settle the group.</summary>
    [HttpGet]
    [ProducesResponseType<GroupBalancesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupBalancesDto>> Get(Guid groupId, CancellationToken cancellationToken) =>
        Ok(await balances.GetAsync(CurrentUserId, groupId, cancellationToken));
}
