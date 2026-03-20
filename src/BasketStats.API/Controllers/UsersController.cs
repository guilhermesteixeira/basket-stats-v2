namespace BasketStats.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BasketStats.API.Requests;
using BasketStats.Application.Commands;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registers the authenticated Keycloak user in the system.
    /// Must be called once before the user can perform any actions (e.g. create teams, matches).
    /// </summary>
    [HttpPost("me")]
    public async Task<IActionResult> RegisterMe([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var userId = await mediator.Send(new CreateUserCommand(request.Email, request.Name, request.KeycloakId), ct);
        return Ok(new { id = userId });
    }
}
