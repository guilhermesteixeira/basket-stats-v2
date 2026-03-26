namespace BasketStats.API.Controllers;

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BasketStats.API.Requests;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Queries;
using BasketStats.Domain.Enums;

[Authorize]
[ApiController]
[Route("api/matches")]
public class MatchesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var command = new CreateMatchCommand(
            request.HomeTeamId,
            request.AwayTeamId,
            userId,
            request.HomePlayers?.Select(p => new PlayerInput(p.Name, p.Number)).ToList(),
            request.AwayPlayers?.Select(p => new PlayerInput(p.Name, p.Number)).ToList());
        var matchId = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetMatch), new { id = matchId }, new { id = matchId });
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMatch(string id, CancellationToken ct)
    {
        var matchDto = await mediator.Send(new GetMatchQuery(id), ct);
        return matchDto is null ? NotFound() : Ok(matchDto);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ListMatches([FromQuery] string? teamId, [FromQuery] MatchStatus? status, CancellationToken ct)
    {
        var list = await mediator.Send(new ListMatchesQuery(teamId, status), ct);
        return Ok(list);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartMatch(string id, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            await mediator.Send(new StartMatchCommand(id, userId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> FinishMatch(string id, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            await mediator.Send(new FinishMatchCommand(id, userId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/events")]
    public async Task<IActionResult> AddEvent(string id, [FromBody] AddEventRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var command = new AddEventCommand
            {
                MatchId = id,
                TeamId = request.TeamId,
                PlayerId = request.PlayerId ?? string.Empty,
                Type = request.Type,
                PeriodNumber = request.PeriodNumber,
                PeriodTimestamp = request.PeriodTimestamp,
                RequestedByUserId = userId,
                CoordinatesX = request.CoordinatesX,
                CoordinatesY = request.CoordinatesY,
                Points = request.Points,
                Made = request.Made,
                FoulType = request.FoulType,
                PlayerFouledId = request.PlayerFouledId,
                Flagrant = request.Flagrant,
                PlayerOutId = request.PlayerOutId
            };
            var eventId = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetMatch), new { id }, new { id = eventId });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
