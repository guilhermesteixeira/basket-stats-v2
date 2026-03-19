namespace BasketStats.Domain.Tests.EntityTests;

using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

public class MatchTests
{
    [Fact]
    public void Create_WithValidTeams_Succeeds()
    {
        // Act
        var match = Match.Create("team-1", "team-2");

        // Assert
        Assert.NotNull(match.Id);
        Assert.Equal("team-1", match.HomeTeamId);
        Assert.Equal("team-2", match.AwayTeamId);
        Assert.Equal(MatchStatus.Scheduled, match.Status);
        Assert.Null(match.StartedAt);
        Assert.Null(match.FinishedAt);
        Assert.Empty(match.Events);
        Assert.NotEmpty(match.Periods);
        Assert.Equal(4, match.Periods.Count);
    }

    [Fact]
    public void Create_WithEmptyHomeTeamId_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Match.Create("", "team-2"));
    }

    [Fact]
    public void Create_WithEmptyAwayTeamId_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Match.Create("team-1", ""));
    }

    [Fact]
    public void Create_WithSameTeams_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => Match.Create("team-1", "team-1"));
    }

    [Fact]
    public void Start_WithScheduledMatch_Succeeds()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");

        // Act
        match.Start();

        // Assert
        Assert.Equal(MatchStatus.Active, match.Status);
        Assert.NotNull(match.StartedAt);
    }

    [Fact]
    public void Start_WithActiveMatch_Throws()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => match.Start());
    }

    [Fact]
    public void Finish_WithActiveMatch_Succeeds()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();

        // Act
        match.Finish();

        // Assert
        Assert.Equal(MatchStatus.Finished, match.Status);
        Assert.NotNull(match.FinishedAt);
    }

    [Fact]
    public void Finish_WithScheduledMatch_Throws()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => match.Finish());
    }

    [Fact]
    public void AddEvent_WithActiveMatch_Succeeds()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();
        var coordinates = new Coordinates(50, 50);
        var scoreEvent = new ScoreEvent("team-1", "player-1", 2, coordinates, PeriodNumber.One, 100);

        // Act
        match.AddEvent(scoreEvent);

        // Assert
        Assert.Single(match.Events);
        Assert.Equal(scoreEvent, match.Events[0]);
    }

    [Fact]
    public void AddEvent_WithScheduledMatch_Throws()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        var coordinates = new Coordinates(50, 50);
        var scoreEvent = new ScoreEvent("team-1", "player-1", 2, coordinates, PeriodNumber.One, 100);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => match.AddEvent(scoreEvent));
    }

    [Fact]
    public void AddEvent_WithFinishedMatch_Throws()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();
        match.Finish();
        var coordinates = new Coordinates(50, 50);
        var scoreEvent = new ScoreEvent("team-1", "player-1", 2, coordinates, PeriodNumber.One, 100);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => match.AddEvent(scoreEvent));
    }

    [Fact]
    public void GetTeamFoulCount_WithNoFouls_ReturnsZero()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();

        // Act
        var count = match.GetTeamFoulCount("team-1", PeriodNumber.One);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void GetTeamFoulCount_WithFouls_ReturnsCorrectCount()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();
        
        var foulEvent1 = new FoulEvent("team-1", "player-1", FoulType.Personal, "player-2", false, PeriodNumber.One, 100);
        var foulEvent2 = new FoulEvent("team-1", "player-1", FoulType.Personal, "player-2", false, PeriodNumber.One, 200);
        var foulEvent3 = new FoulEvent("team-2", "player-3", FoulType.Personal, "player-1", false, PeriodNumber.One, 300);
        
        match.AddEvent(foulEvent1);
        match.AddEvent(foulEvent2);
        match.AddEvent(foulEvent3);

        // Act
        var team1Count = match.GetTeamFoulCount("team-1", PeriodNumber.One);
        var team2Count = match.GetTeamFoulCount("team-2", PeriodNumber.One);

        // Assert
        Assert.Equal(2, team1Count);
        Assert.Equal(1, team2Count);
    }

    [Fact]
    public void GetTeamFoulCount_WithDifferentPeriods_CountsOnlyRequestedPeriod()
    {
        // Arrange
        var match = Match.Create("team-1", "team-2");
        match.Start();
        
        var foulEvent1 = new FoulEvent("team-1", "player-1", FoulType.Personal, "player-2", false, PeriodNumber.One, 100);
        var foulEvent2 = new FoulEvent("team-1", "player-1", FoulType.Personal, "player-2", false, PeriodNumber.Two, 100);
        
        match.AddEvent(foulEvent1);
        match.AddEvent(foulEvent2);

        // Act
        var period1Count = match.GetTeamFoulCount("team-1", PeriodNumber.One);
        var period2Count = match.GetTeamFoulCount("team-1", PeriodNumber.Two);

        // Assert
        Assert.Equal(1, period1Count);
        Assert.Equal(1, period2Count);
    }
}
