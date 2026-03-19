namespace BasketStats.API.Tests.ControllerTests;

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using BasketStats.API.Controllers;
using BasketStats.API.Requests;
using BasketStats.Application.Commands;
using BasketStats.Application.DTOs;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Queries;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;

public class MatchesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private MatchesController CreateController(string userId = "user-1")
    {
        var controller = new MatchesController(_mediator.Object);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    // TC-MATCH-001: Successfully create match
    [Fact]
    public async Task CreateMatch_ValidRequest_Returns201WithLocation()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<CreateMatchCommand>(), default))
            .ReturnsAsync("match-123");

        // Act
        var result = await CreateController().CreateMatch(
            new CreateMatchRequest("home-1", "away-1"), default);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    // TC-MATCH-006: Retrieve existing match by ID
    [Fact]
    public async Task GetMatch_ExistingMatch_Returns200WithMatchDto()
    {
        // Arrange
        var dto = new MatchDto { Id = "match-1", HomeTeamId = "home-1", AwayTeamId = "away-1", Status = "Scheduled" };
        _mediator.Setup(m => m.Send(It.IsAny<GetMatchQuery>(), default)).ReturnsAsync(dto);

        // Act
        var result = await CreateController().GetMatch("match-1", default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    // TC-MATCH-007: Fail to retrieve non-existent match (404)
    [Fact]
    public async Task GetMatch_NotFound_Returns404()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetMatchQuery>(), default))
            .ReturnsAsync((MatchDto?)null);

        // Act
        var result = await CreateController().GetMatch("no-match", default);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ListMatches_NoFilter_Returns200WithList()
    {
        // Arrange
        var list = new List<MatchDto> { new() { Id = "m1" }, new() { Id = "m2" } };
        _mediator.Setup(m => m.Send(It.IsAny<ListMatchesQuery>(), default)).ReturnsAsync(list);

        // Act
        var result = await CreateController().ListMatches(null, null, default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }

    // TC-MATCH-009: Filter matches by team — verifies TeamId is passed to query
    [Fact]
    public async Task ListMatches_WithTeamFilter_PassesTeamIdToQuery()
    {
        // Arrange
        var list = new List<MatchDto> { new() { Id = "m1", HomeTeamId = "team-1" } };
        _mediator.Setup(m => m.Send(
            It.Is<ListMatchesQuery>(q => q.TeamId == "team-1"),
            default)).ReturnsAsync(list);

        // Act
        var result = await CreateController().ListMatches("team-1", null, default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
        _mediator.Verify(m => m.Send(
            It.Is<ListMatchesQuery>(q => q.TeamId == "team-1"),
            default), Times.Once);
    }

    // TC-MATCH-010: Filter matches by status — verifies Status is passed to query
    [Fact]
    public async Task ListMatches_WithStatusFilter_PassesStatusToQuery()
    {
        // Arrange
        var list = new List<MatchDto> { new() { Id = "m2", Status = "Active" } };
        _mediator.Setup(m => m.Send(
            It.Is<ListMatchesQuery>(q => q.Status == MatchStatus.Active),
            default)).ReturnsAsync(list);

        // Act
        var result = await CreateController().ListMatches(null, MatchStatus.Active, default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
        _mediator.Verify(m => m.Send(
            It.Is<ListMatchesQuery>(q => q.Status == MatchStatus.Active),
            default), Times.Once);
    }

    [Fact]
    public async Task StartMatch_Success_Returns204()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<StartMatchCommand>(), default))
            .Returns(Task.FromResult(MediatR.Unit.Value));

        // Act
        var result = await CreateController().StartMatch("match-1", default);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task StartMatch_MatchNotFound_Returns404()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<StartMatchCommand>(), default))
            .ThrowsAsync(new NotFoundException("Match not found"));

        // Act
        var result = await CreateController().StartMatch("no-match", default);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // TC-MATCH-012: Fail to update with invalid status transition
    [Fact]
    public async Task StartMatch_InvalidStateTransition_Returns400()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<StartMatchCommand>(), default))
            .ThrowsAsync(new InvalidOperationException("Can only start a scheduled match"));

        // Act
        var result = await CreateController().StartMatch("match-1", default);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task FinishMatch_Success_Returns204()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<FinishMatchCommand>(), default))
            .Returns(Task.FromResult(MediatR.Unit.Value));

        // Act
        var result = await CreateController().FinishMatch("match-1", default);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    // TC-EVENT-001: Add score event for own team — returns 201
    [Fact]
    public async Task AddEvent_Success_Returns201()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<AddEventCommand>(), default))
            .ReturnsAsync("event-123");

        var request = new AddEventRequest
        {
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Score,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            Points = 2,
            CoordinatesX = 50,
            CoordinatesY = 50
        };

        // Act
        var result = await CreateController().AddEvent("match-1", request, default);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    // TC-AUTH-009: Opponent adds score event — returns 201 (opponent tracking is allowed)
    [Fact]
    public async Task AddEvent_OpponentAddsScore_Returns201()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<AddEventCommand>(), default))
            .ReturnsAsync("event-456");

        var request = new AddEventRequest
        {
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Score,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            Points = 2,
            CoordinatesX = 50,
            CoordinatesY = 50
        };

        // Act — away team owner (user-away) adding a score event for the home team
        var result = await CreateController(userId: "user-away").AddEvent("match-1", request, default);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task AddEvent_ForbiddenException_Returns403()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<AddEventCommand>(), default))
            .ThrowsAsync(new ForbiddenException("Not authorized"));

        var request = new AddEventRequest
        {
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Score,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            Points = 2,
            CoordinatesX = 50,
            CoordinatesY = 50
        };

        // Act
        var result = await CreateController().AddEvent("match-1", request, default);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task AddEvent_InvalidOperationException_Returns400()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<AddEventCommand>(), default))
            .ThrowsAsync(new InvalidOperationException("Match not active"));

        var request = new AddEventRequest
        {
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Score,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            Points = 2,
            CoordinatesX = 50,
            CoordinatesY = 50
        };

        // Act
        var result = await CreateController().AddEvent("match-1", request, default);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
