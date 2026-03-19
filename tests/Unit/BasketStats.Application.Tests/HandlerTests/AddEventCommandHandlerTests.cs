namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;

using DomainMatch = BasketStats.Domain.Entities.Match;

public class AddEventCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private AddEventCommandHandler CreateHandler() =>
        new(_matchRepo.Object, _teamRepo.Object, _userRepo.Object);

    private static DomainMatch CreateActiveMatch(string homeTeamId = "home-1", string awayTeamId = "away-1")
    {
        var match = DomainMatch.Create(homeTeamId, awayTeamId);
        match.Start();
        return match;
    }

    private AddEventCommand BuildScoreCommand(string matchId, string teamId, string userId,
        decimal? coordinatesX = 50, decimal? coordinatesY = 50) => new()
    {
        MatchId = matchId,
        TeamId = teamId,
        PlayerId = "player-1",
        Type = EventType.Score,
        PeriodNumber = PeriodNumber.One,
        PeriodTimestamp = 60,
        RequestedByUserId = userId,
        Points = 2,
        CoordinatesX = coordinatesX,
        CoordinatesY = coordinatesY
    };

    private void SetupMatchAndTeams(DomainMatch match, Team homeTeam, Team awayTeam, User user)
    {
        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync(homeTeam.Id, default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync(awayTeam.Id, default)).ReturnsAsync(awayTeam);
        _matchRepo.Setup(r => r.SaveAsync(match, default)).Returns(Task.CompletedTask);
    }

    // TC-EVENT-001
    [Fact]
    public async Task Handle_HomeOwnerAddsScoreForHomeTeam_AddsEvent()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-1");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-002
    [Fact]
    public async Task Handle_HomeOwnerAddsFoulForHomeTeam_AddsEvent()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Foul,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            RequestedByUserId = "user-1",
            FoulType = FoulType.Personal,
            PlayerFouledId = "player-2",
            Flagrant = false
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-003
    [Fact]
    public async Task Handle_HomeOwnerAddsSubstitution_ForOwnTeam_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-in",
            Type = EventType.Substitution,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 120,
            RequestedByUserId = "user-1",
            PlayerOutId = "player-out"
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
        Assert.IsType<SubstitutionEvent>(match.Events[0]);
    }

    // TC-EVENT-004
    [Fact]
    public async Task Handle_HomeOwnerAddsMissedShot_WithCoordinates_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.MissedShot,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 90,
            RequestedByUserId = "user-1",
            CoordinatesX = 40,
            CoordinatesY = 60
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
        Assert.IsType<MissedShotEvent>(match.Events[0]);
    }

    // TC-EVENT-005
    [Fact]
    public async Task Handle_HomeOwnerAddsFreeThrow_Made_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.FreeThrow,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 150,
            RequestedByUserId = "user-1",
            Made = true,
            FoulType = FoulType.Personal
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
        var ftEvent = Assert.IsType<FreeThrowEvent>(match.Events[0]);
        Assert.True(ftEvent.Made);
    }

    // TC-EVENT-006: Admin user bypasses authorization restrictions
    [Fact]
    public async Task Handle_AdminUser_CanAddAnyEventType_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "owner-away");
        var adminUser = User.Create("admin-user", "admin@t.com", "Admin", "kc-admin");
        adminUser.AddRole("admin");
        SetupMatchAndTeams(match, homeTeam, awayTeam, adminUser);

        // Admin adding a Foul to the opponent team (normally forbidden for non-owners)
        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Foul,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            RequestedByUserId = "admin-user",
            FoulType = FoulType.Personal,
            PlayerFouledId = "player-2",
            Flagrant = false
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-007: Away owner tracks opponent's score (allowed)
    [Fact]
    public async Task Handle_AwayOwnerAddsScore_ForOpponentTeam_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-away");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-008: Away owner tracks opponent's missed shot (allowed)
    [Fact]
    public async Task Handle_AwayOwnerAddsMissedShot_ForOpponentTeam_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.MissedShot,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 90,
            RequestedByUserId = "user-away",
            CoordinatesX = 40,
            CoordinatesY = 60
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-009: Away owner tracks opponent's free throw (allowed)
    [Fact]
    public async Task Handle_AwayOwnerAddsFreeThrow_ForOpponentTeam_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.FreeThrow,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 150,
            RequestedByUserId = "user-away",
            Made = true,
            FoulType = FoulType.Personal
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-010: Away owner cannot add foul to opponent team
    [Fact]
    public async Task Handle_AwayOwnerAddsFoulForHomeTeam_ThrowsForbiddenException()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-away", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Foul,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            RequestedByUserId = "user-away",
            FoulType = FoulType.Personal,
            PlayerFouledId = "player-2",
            Flagrant = false
        };

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, default));
    }

    // TC-EVENT-011: Away owner cannot add substitution to opponent team
    [Fact]
    public async Task Handle_AwayOwnerAddsSubstitution_ForOpponentTeam_ThrowsForbiddenException()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-away", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-in",
            Type = EventType.Substitution,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 120,
            RequestedByUserId = "user-away",
            PlayerOutId = "player-out"
        };

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, default));
    }

    // TC-EVENT-016: Score event without coordinates throws
    [Fact]
    public async Task Handle_ScoreEvent_WithoutCoordinates_ThrowsException()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-1",
            coordinatesX: null, coordinatesY: null);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(command, default));
    }

    // TC-EVENT-017: Missed shot event without coordinates throws
    [Fact]
    public async Task Handle_MissedShotEvent_WithoutCoordinates_ThrowsException()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.MissedShot,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 90,
            RequestedByUserId = "user-1",
            CoordinatesX = null,
            CoordinatesY = null
        };

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(command, default));
    }

    // TC-EVENT-018: Free throw does NOT require coordinates
    [Fact]
    public async Task Handle_FreeThrowEvent_WithoutCoordinates_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.FreeThrow,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 150,
            RequestedByUserId = "user-1",
            Made = true,
            FoulType = FoulType.Personal,
            CoordinatesX = null,
            CoordinatesY = null
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-019: Foul event does NOT require coordinates
    [Fact]
    public async Task Handle_FoulEvent_WithoutCoordinates_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Foul,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            RequestedByUserId = "user-1",
            FoulType = FoulType.Personal,
            PlayerFouledId = "player-2",
            Flagrant = false,
            CoordinatesX = null,
            CoordinatesY = null
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-020: Substitution event does NOT require coordinates
    [Fact]
    public async Task Handle_SubstitutionEvent_WithoutCoordinates_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-in",
            Type = EventType.Substitution,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 120,
            RequestedByUserId = "user-1",
            PlayerOutId = "player-out",
            CoordinatesX = null,
            CoordinatesY = null
        };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    // TC-EVENT-021: Fail to add event to finished match
    [Fact]
    public async Task Handle_FinishedMatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();
        match.Finish();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-1");

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(command, default));
    }

    // TC-EVENT-022: Fail to add event to non-existent match
    [Fact]
    public async Task Handle_MatchNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        var command = BuildScoreCommand("no-match", "home-1", "user-1");

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_UnrelatedUser_ThrowsForbiddenException()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "owner-away");
        var user = User.Create("user-x", "x@t.com", "Other User", "kc-x");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-x", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-x");

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, default));
    }

    // TC-EVENT-047: Record event with period number (1-4)
    [Fact]
    public async Task Handle_EventWithValidPeriodNumber_Succeeds()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-1");
        command = command with { PeriodNumber = PeriodNumber.Four, PeriodTimestamp = 500 };

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(PeriodNumber.Four, match.Events[0].PeriodNumber);
    }

    // TC-EVENT-049 / TC-EVENT-050: Reject event with invalid period timestamp (outside 0-600s)
    [Fact]
    public async Task Handle_EventWithInvalidPeriodTimestamp_ThrowsArgumentException()
    {
        // Arrange
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");
        SetupMatchAndTeams(match, homeTeam, awayTeam, user);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-1");
        command = command with { PeriodTimestamp = 700 };

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => CreateHandler().Handle(command, default));
    }
}
