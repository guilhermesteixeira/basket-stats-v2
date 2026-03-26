namespace BasketStats.API.Controllers;

using System.Security.Claims;
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
    /// Registers or retrieves the authenticated Keycloak user.
    /// Reads user info directly from JWT claims — no body required.
    /// Safe to call on every login (idempotent).
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(CancellationToken ct)
    {
        var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub")
                         ?? string.Empty;
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email")
                    ?? string.Empty;
        var name = User.FindFirstValue("name")
                   ?? User.FindFirstValue(ClaimTypes.Name)
                   ?? email;

        var userId = await mediator.Send(new CreateUserCommand(email, name, keycloakId), ct);
        return Ok(new { id = userId });
    }

    /// <summary>
    /// Registers the authenticated Keycloak user in the system (body-based, kept for compatibility).
    /// </summary>
    [HttpPost("me")]
    public async Task<IActionResult> RegisterMe([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var userId = await mediator.Send(new CreateUserCommand(request.Email, request.Name, request.KeycloakId), ct);
        return Ok(new { id = userId });
    }
}
