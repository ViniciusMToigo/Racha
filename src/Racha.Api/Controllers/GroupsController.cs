using Microsoft.AspNetCore.Mvc;
using Racha.Application.Groups;
using Racha.Web.Common.Controllers;

namespace Racha.Api.Controllers;

/// <summary>Groups of people who share expenses. Every action requires a valid access token.</summary>
[Route("api/groups")]
public sealed class GroupsController(IGroupService groups) : ApiControllerBase
{
    /// <summary>Lists the groups the current user belongs to.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GroupSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GroupSummaryDto>>> ListMine(CancellationToken cancellationToken) =>
        Ok(await groups.ListMineAsync(CurrentUserId, cancellationToken));

    /// <summary>Creates a group. The current user becomes its creator and first member.</summary>
    [HttpPost]
    [ProducesResponseType<GroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupDto>> Create(CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await groups.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { groupId = group.Id }, group);
    }

    /// <summary>Returns a group and its members. Only members can see it.</summary>
    [HttpGet("{groupId:guid}")]
    [ProducesResponseType<GroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupDto>> Get(Guid groupId, CancellationToken cancellationToken) =>
        Ok(await groups.GetAsync(CurrentUserId, groupId, cancellationToken));

    /// <summary>Renames a group. Only its creator can do this.</summary>
    [HttpPut("{groupId:guid}")]
    [ProducesResponseType<GroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupDto>> Update(Guid groupId, UpdateGroupRequest request, CancellationToken cancellationToken) =>
        Ok(await groups.UpdateAsync(CurrentUserId, groupId, request, cancellationToken));

    /// <summary>Deletes a group with all its expenses. Only its creator can do this.</summary>
    [HttpDelete("{groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid groupId, CancellationToken cancellationToken)
    {
        await groups.DeleteAsync(CurrentUserId, groupId, cancellationToken);
        return NoContent();
    }

    /// <summary>Adds a registered user to the group by e-mail. Any member can do this.</summary>
    [HttpPost("{groupId:guid}/members")]
    [ProducesResponseType<GroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupDto>> AddMember(Guid groupId, AddMemberRequest request, CancellationToken cancellationToken) =>
        Ok(await groups.AddMemberAsync(CurrentUserId, groupId, request, cancellationToken));
}
