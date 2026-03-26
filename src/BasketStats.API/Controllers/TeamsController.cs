namespace BasketStats.API.Controllers;

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BasketStats.API.Requests;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Queries;

[Authorize]
[ApiController]
[Route("api/teams")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ListTeams(CancellationToken ct)
    {
        var teams = await mediator.Send(new ListTeamsQuery(), ct);
        return Ok(teams);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        try
        {
            var teamId = await mediator.Send(new CreateTeamCommand(request.Name, userId), ct);
            return CreatedAtAction(nameof(CreateTeam), new { id = teamId }, new { id = teamId });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
