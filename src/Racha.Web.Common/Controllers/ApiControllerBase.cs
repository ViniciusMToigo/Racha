using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Racha.Web.Common.Security;

namespace Racha.Web.Common.Controllers;

/// <summary>
/// Base for every API controller: JSON in/out, authentication required unless an action opts out
/// with [AllowAnonymous], and easy access to the current user's id.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId => User.GetRequiredUserId();
}
