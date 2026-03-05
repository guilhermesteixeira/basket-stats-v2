using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BasketStats.API.Models;

namespace BasketStats.API.Controllers;

[ApiController]
[Route("api/matches/{matchId}/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;

    public EventsController(ILogger<EventsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all events for a match
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MatchEvent>>> GetEvents(string matchId)
    {
        _logger.LogInformation("Getting events for match {MatchId}", matchId);
        
        // TODO: Implement Firestore query
        return Ok(new List<MatchEvent>());
    }

    /// <summary>
    /// Add score event (2 or 3 points)
    /// </summary>
    [HttpPost("score")]
    public async Task<ActionResult<ScoreEvent>> AddScoreEvent(string matchId, [FromBody] AddScoreEventRequest request)
    {
        _logger.LogInformation("Adding score event to match {MatchId}", matchId);
        
        // Validate: Score events must have coordinates
        if (request.CoordinateX < 0 || request.CoordinateX > 100 || 
            request.CoordinateY < 0 || request.CoordinateY > 100)
        {
            return BadRequest("Coordinates must be between 0 and 100");
        }

        // TODO: Check ownership rules (team owner or opponent)
        // TODO: Save to Firestore
        
        return CreatedAtAction(nameof(GetEvents), new { matchId }, new ScoreEvent());
    }

    /// <summary>
    /// Add missed shot event
    /// </summary>
    [HttpPost("missed-shot")]
    public async Task<ActionResult<MissedShotEvent>> AddMissedShotEvent(string matchId, [FromBody] AddMissedShotEventRequest request)
    {
        _logger.LogInformation("Adding missed shot event to match {MatchId}", matchId);
        
        // TODO: Implement
        return CreatedAtAction(nameof(GetEvents), new { matchId }, new MissedShotEvent());
    }

    /// <summary>
    /// Add free throw event
    /// </summary>
    [HttpPost("free-throw")]
    public async Task<ActionResult<FreeThrowEvent>> AddFreeThrowEvent(string matchId, [FromBody] AddFreeThrowEventRequest request)
    {
        _logger.LogInformation("Adding free throw event to match {MatchId}", matchId);
        
        // TODO: Implement
        return CreatedAtAction(nameof(GetEvents), new { matchId }, new FreeThrowEvent());
    }

    /// <summary>
    /// Add foul event
    /// </summary>
    [HttpPost("foul")]
    public async Task<ActionResult<FoulEvent>> AddFoulEvent(string matchId, [FromBody] AddFoulEventRequest request)
    {
        _logger.LogInformation("Adding foul event to match {MatchId}", matchId);
        
        // TODO: Implement
        return CreatedAtAction(nameof(GetEvents), new { matchId }, new FoulEvent());
    }

    /// <summary>
    /// Add substitution event
    /// </summary>
    [HttpPost("substitution")]
    public async Task<ActionResult<SubstitutionEvent>> AddSubstitutionEvent(string matchId, [FromBody] AddSubstitutionEventRequest request)
    {
        _logger.LogInformation("Adding substitution event to match {MatchId}", matchId);
        
        // TODO: Implement
        return CreatedAtAction(nameof(GetEvents), new { matchId }, new SubstitutionEvent());
    }
}

public class AddScoreEventRequest
{
    public int Points { get; set; } // 2 or 3
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public int PeriodNumber { get; set; }
    public int PeriodTimestampSeconds { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
}

public class AddMissedShotEventRequest
{
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public int PeriodNumber { get; set; }
    public int PeriodTimestampSeconds { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
}

public class AddFreeThrowEventRequest
{
    public bool Made { get; set; }
    public FoulType FoulType { get; set; }
    public int PeriodNumber { get; set; }
    public int PeriodTimestampSeconds { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
}

public class AddFoulEventRequest
{
    public FoulType FoulType { get; set; }
    public string PlayerFouledId { get; set; } = string.Empty;
    public bool Flagrant { get; set; }
    public int PeriodNumber { get; set; }
    public int PeriodTimestampSeconds { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
}

public class AddSubstitutionEventRequest
{
    public string SubstitutePlayerId { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public int PeriodTimestampSeconds { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
}
