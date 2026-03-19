using BasketStats.Domain.Entities;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;
using BasketStats.Infrastructure.Mapping;
using BasketStats.Infrastructure.Models;

namespace BasketStats.Infrastructure.Tests.Mapping;

public class MatchFirestoreMapperTests
{
    private static Match CreateScheduledMatch()
        => Match.Create("home-team-1", "away-team-2");

    private static Match CreateActiveMatch()
    {
        var match = Match.Create("home-team-1", "away-team-2");
        match.Start();
        return match;
    }

    [Fact]
    public void ToDocument_Match_MapsAllScalarProperties()
    {
        // Arrange
        var match = CreateScheduledMatch();

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        Assert.Equal(match.Id.Value, doc.Id);
        Assert.Equal(match.HomeTeamId, doc.HomeTeamId);
        Assert.Equal(match.AwayTeamId, doc.AwayTeamId);
        Assert.Equal((int)match.Status, doc.Status);
        Assert.Equal(match.CreatedAt, doc.CreatedAt);
        Assert.Null(doc.StartedAt);
        Assert.Null(doc.FinishedAt);
    }

    [Fact]
    public void ToDocument_ActiveMatch_MapsStartedAt()
    {
        // Arrange
        var match = CreateActiveMatch();

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        Assert.NotNull(doc.StartedAt);
        Assert.Equal(match.StartedAt, doc.StartedAt);
    }

    [Fact]
    public void ToDocument_FinishedMatch_MapsFinishedAt()
    {
        // Arrange
        var match = CreateActiveMatch();
        match.Finish();

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        Assert.NotNull(doc.FinishedAt);
        Assert.Equal(match.FinishedAt, doc.FinishedAt);
    }

    [Fact]
    public void ToDocument_MatchWithScoreEvent_MapsEventCorrectly()
    {
        // Arrange
        var match = CreateActiveMatch();
        var scoreEvent = new ScoreEvent("home-team-1", "player-1", 3,
            new Coordinates(25.5m, 50.0m), PeriodNumber.One, 120);
        match.AddEvent(scoreEvent);

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        var eventDoc = Assert.Single(doc.Events);
        Assert.Equal((int)EventType.Score, eventDoc.Type);
        Assert.Equal(3, eventDoc.Points);
        Assert.Equal(25.5, eventDoc.CoordinatesX);
        Assert.Equal(50.0, eventDoc.CoordinatesY);
    }

    [Fact]
    public void ToDocument_MatchWithFreeThrowEvent_MapsEventCorrectly()
    {
        // Arrange
        var match = CreateActiveMatch();
        var ftEvent = new FreeThrowEvent("home-team-1", "player-1", true,
            FoulType.Personal, PeriodNumber.Two, 300);
        match.AddEvent(ftEvent);

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        var eventDoc = Assert.Single(doc.Events);
        Assert.Equal((int)EventType.FreeThrow, eventDoc.Type);
        Assert.True(eventDoc.Made);
        Assert.Equal((int)FoulType.Personal, eventDoc.FoulType);
        Assert.Null(eventDoc.CoordinatesX);
        Assert.Null(eventDoc.CoordinatesY);
    }

    [Fact]
    public void ToDocument_MatchWithFoulEvent_MapsEventCorrectly()
    {
        // Arrange
        var match = CreateActiveMatch();
        var foulEvent = new FoulEvent("home-team-1", "player-1",
            FoulType.Technical, "player-2", false, PeriodNumber.Three, 450);
        match.AddEvent(foulEvent);

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        var eventDoc = Assert.Single(doc.Events);
        Assert.Equal((int)EventType.Foul, eventDoc.Type);
        Assert.Equal((int)FoulType.Technical, eventDoc.FoulType);
        Assert.Equal("player-2", eventDoc.PlayerFouledId);
        Assert.False(eventDoc.Flagrant);
    }

    [Fact]
    public void ToDocument_MatchWithMissedShotEvent_MapsEventCorrectly()
    {
        // Arrange
        var match = CreateActiveMatch();
        var missedEvent = new MissedShotEvent("away-team-2", "player-3",
            new Coordinates(75.0m, 30.0m), PeriodNumber.Four, 500);
        match.AddEvent(missedEvent);

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        var eventDoc = Assert.Single(doc.Events);
        Assert.Equal((int)EventType.MissedShot, eventDoc.Type);
        Assert.Equal(75.0, eventDoc.CoordinatesX);
        Assert.Equal(30.0, eventDoc.CoordinatesY);
        Assert.Null(eventDoc.Points);
    }

    [Fact]
    public void ToDocument_MatchWithSubstitutionEvent_MapsEventCorrectly()
    {
        // Arrange
        var match = CreateActiveMatch();
        var subEvent = new SubstitutionEvent("home-team-1", "player-in", "player-out",
            PeriodNumber.One, 180);
        match.AddEvent(subEvent);

        // Act
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Assert
        var eventDoc = Assert.Single(doc.Events);
        Assert.Equal((int)EventType.Substitution, eventDoc.Type);
        Assert.Equal("player-out", eventDoc.PlayerOutId);
    }

    [Fact]
    public void ToDomain_Document_ReconstructsMatchCorrectly()
    {
        // Arrange
        var match = CreateActiveMatch();
        var scoreEvent = new ScoreEvent("home-team-1", "player-1", 2,
            new Coordinates(50.0m, 50.0m), PeriodNumber.One, 60);
        match.AddEvent(scoreEvent);
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Act
        var result = MatchFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(match.Id.Value, result.Id.Value);
        Assert.Equal(match.HomeTeamId, result.HomeTeamId);
        Assert.Equal(match.AwayTeamId, result.AwayTeamId);
        Assert.Equal(match.Status, result.Status);
        Assert.Equal(match.CreatedAt, result.CreatedAt);
        Assert.Equal(match.StartedAt, result.StartedAt);
        Assert.Single(result.Events);
    }

    [Fact]
    public void ToDomain_Document_ReconstructsAllEventTypes()
    {
        // Arrange
        var match = CreateActiveMatch();
        match.AddEvent(new ScoreEvent("home-team-1", "p1", 2, new Coordinates(25m, 25m), PeriodNumber.One, 60));
        match.AddEvent(new MissedShotEvent("home-team-1", "p1", new Coordinates(50m, 50m), PeriodNumber.One, 120));
        match.AddEvent(new FreeThrowEvent("home-team-1", "p1", true, FoulType.Personal, PeriodNumber.One, 180));
        match.AddEvent(new FoulEvent("home-team-1", "p1", FoulType.Technical, "p2", false, PeriodNumber.One, 240));
        match.AddEvent(new SubstitutionEvent("home-team-1", "p3", "p1", PeriodNumber.One, 300));
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Act
        var result = MatchFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(5, result.Events.Count);
        Assert.IsType<ScoreEvent>(result.Events[0]);
        Assert.IsType<MissedShotEvent>(result.Events[1]);
        Assert.IsType<FreeThrowEvent>(result.Events[2]);
        Assert.IsType<FoulEvent>(result.Events[3]);
        Assert.IsType<SubstitutionEvent>(result.Events[4]);
    }

    [Fact]
    public void ToDomain_Document_PreservesEventId()
    {
        // Arrange
        var match = CreateActiveMatch();
        var scoreEvent = new ScoreEvent("home-team-1", "player-1", 3,
            new Coordinates(25m, 75m), PeriodNumber.Two, 200);
        match.AddEvent(scoreEvent);
        var originalEventId = scoreEvent.Id.Value;
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Act
        var result = MatchFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(originalEventId, result.Events[0].Id.Value);
    }

    [Fact]
    public void ToDomain_Document_PreservesPeriods()
    {
        // Arrange
        var match = CreateActiveMatch();
        var doc = MatchFirestoreMapper.ToDocument(match);

        // Act
        var result = MatchFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(4, result.Periods.Count);
        Assert.Equal(PeriodNumber.One, result.Periods[0].Number);
        Assert.Equal(PeriodNumber.Two, result.Periods[1].Number);
        Assert.Equal(PeriodNumber.Three, result.Periods[2].Number);
        Assert.Equal(PeriodNumber.Four, result.Periods[3].Number);
    }
}
