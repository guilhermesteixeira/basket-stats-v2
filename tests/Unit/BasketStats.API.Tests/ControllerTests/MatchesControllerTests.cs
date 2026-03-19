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

    [Fact]
    public async Task CreateMatch_ValidRequest_Returns201WithLocation()
    {
        _mediator.Setup(m => m.Send(It.IsAny<CreateMatchCommand>(), default))
            .ReturnsAsync("match-123");

        var result = await CreateController().CreateMatch(
            new CreateMatchRequest("home-1", "away-1"), default);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task GetMatch_ExistingMatch_Returns200WithMatchDto()
    {
        var dto = new MatchDto { Id = "match-1", HomeTeamId = "home-1", AwayTeamId = "away-1", Status = "Scheduled" };
        _mediator.Setup(m => m.Send(It.IsAny<GetMatchQuery>(), default)).ReturnsAsync(dto);

        var result = await CreateController().GetMatch("match-1", default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetMatch_NotFound_Returns404()
    {
        _mediator.Setup(m => m.Send(It.IsAny<GetMatchQuery>(), default))
            .ReturnsAsync((MatchDto?)null);

        var result = await CreateController().GetMatch("no-match", default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ListMatches_NoFilter_Returns200WithList()
    {
        var list = new List<MatchDto> { new() { Id = "m1" }, new() { Id = "m2" } };
        _mediator.Setup(m => m.Send(It.IsAny<ListMatchesQuery>(), default)).ReturnsAsync(list);

        var result = await CreateController().ListMatches(null, null, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, ok.Value);
    }

    [Fact]
    public async Task StartMatch_Success_Returns204()
    {
        _mediator.Setup(m => m.Send(It.IsAny<StartMatchCommand>(), default))
            .Returns(Task.FromResult(MediatR.Unit.Value));

        var result = await CreateController().StartMatch("match-1", default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task StartMatch_MatchNotFound_Returns404()
    {
        _mediator.Setup(m => m.Send(It.IsAny<StartMatchCommand>(), default))
            .ThrowsAsync(new NotFoundException("Match not found"));

        var result = await CreateController().StartMatch("no-match", default);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task FinishMatch_Success_Returns204()
    {
        _mediator.Setup(m => m.Send(It.IsAny<FinishMatchCommand>(), default))
            .Returns(Task.FromResult(MediatR.Unit.Value));

        var result = await CreateController().FinishMatch("match-1", default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddEvent_Success_Returns201()
    {
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

        var result = await CreateController().AddEvent("match-1", request, default);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task AddEvent_ForbiddenException_Returns403()
    {
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

        var result = await CreateController().AddEvent("match-1", request, default);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task AddEvent_InvalidOperationException_Returns400()
    {
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

        var result = await CreateController().AddEvent("match-1", request, default);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
