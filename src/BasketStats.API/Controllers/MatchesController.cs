using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BasketStats.API.Models;

namespace BasketStats.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly ILogger<MatchesController> _logger;

    public MatchesController(ILogger<MatchesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all matches (with pagination)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Match>>> GetMatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting matches - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        
        // TODO: Implement pagination and Firestore query
        return Ok(new List<Match>());
    }

    /// <summary>
    /// Get match by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Match>> GetMatch(string id)
    {
        _logger.LogInformation("Getting match {Id}", id);
        
        // TODO: Implement Firestore query
        return NotFound();
    }

    /// <summary>
    /// Create a new match
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "match_creator,admin")]
    public async Task<ActionResult<Match>> CreateMatch([FromBody] CreateMatchRequest request)
    {
        _logger.LogInformation("Creating match from user {UserId}", User.FindFirst("sub")?.Value);
        
        // TODO: Implement match creation with Firestore
        return CreatedAtAction(nameof(GetMatch), new { id = "temp-id" }, new Match());
    }

    /// <summary>
    /// Update match status
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "match_creator,admin")]
    public async Task<IActionResult> UpdateMatch(string id, [FromBody] UpdateMatchRequest request)
    {
        _logger.LogInformation("Updating match {Id}", id);
        
        // TODO: Implement match update
        return NoContent();
    }

    /// <summary>
    /// Delete match
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteMatch(string id)
    {
        _logger.LogInformation("Deleting match {Id}", id);
        
        // TODO: Implement match deletion
        return NoContent();
    }
}

public class CreateMatchRequest
{
    public List<Team> Teams { get; set; } = new();
}

public class UpdateMatchRequest
{
    public MatchStatus Status { get; set; }
}
