namespace BasketStats.Domain.Tests.EntityTests;

using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

public class EventTests
{
    [Fact]
    public void ScoreEvent_WithValidData_Succeeds()
    {
        // Arrange
        var coordinates = new Coordinates(50, 75);

        // Act
        var scoreEvent = new ScoreEvent("team-1", "player-1", 2, coordinates, PeriodNumber.One, 100);

        // Assert
        Assert.NotNull(scoreEvent.Id);
        Assert.Equal("team-1", scoreEvent.TeamId);
        Assert.Equal("player-1", scoreEvent.PlayerId);
        Assert.Equal(2, scoreEvent.Points);
        Assert.Equal(coordinates, scoreEvent.Coordinates);
        Assert.Equal(EventType.Score, scoreEvent.Type);
        Assert.Equal(PeriodNumber.One, scoreEvent.PeriodNumber);
        Assert.Equal(100, scoreEvent.PeriodTimestamp);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    public void ScoreEvent_WithInvalidPoints_Throws(int points)
    {
        // Arrange
        var coordinates = new Coordinates(50, 75);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new ScoreEvent("team-1", "player-1", points, coordinates, PeriodNumber.One, 100));
    }

    [Fact]
    public void ScoreEvent_With3Points_Succeeds()
    {
        // Arrange
        var coordinates = new Coordinates(50, 75);

        // Act
        var scoreEvent = new ScoreEvent("team-1", "player-1", 3, coordinates, PeriodNumber.One, 100);

        // Assert
        Assert.Equal(3, scoreEvent.Points);
    }

    [Fact]
    public void ScoreEvent_WithoutCoordinates_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ScoreEvent("team-1", "player-1", 2, null, PeriodNumber.One, 100));
    }

    [Fact]
    public void MissedShotEvent_WithValidData_Succeeds()
    {
        // Arrange
        var coordinates = new Coordinates(25, 50);

        // Act
        var missedShotEvent = new MissedShotEvent("team-1", "player-1", coordinates, PeriodNumber.Two, 250);

        // Assert
        Assert.NotNull(missedShotEvent.Id);
        Assert.Equal(EventType.MissedShot, missedShotEvent.Type);
        Assert.Equal(coordinates, missedShotEvent.Coordinates);
    }

    [Fact]
    public void MissedShotEvent_WithoutCoordinates_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new MissedShotEvent("team-1", "player-1", null, PeriodNumber.One, 100));
    }

    [Fact]
    public void FreeThrowEvent_WithValidData_Succeeds()
    {
        // Act
        var freeThrowEvent = new FreeThrowEvent("team-1", "player-1", true, FoulType.Personal, PeriodNumber.One, 150);

        // Assert
        Assert.NotNull(freeThrowEvent.Id);
        Assert.Equal(EventType.FreeThrow, freeThrowEvent.Type);
        Assert.True(freeThrowEvent.Made);
        Assert.Equal(FoulType.Personal, freeThrowEvent.FoulType);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FreeThrowEvent_WithMadeAndMissed_Succeeds(bool made)
    {
        // Act
        var freeThrowEvent = new FreeThrowEvent("team-1", "player-1", made, FoulType.Technical, PeriodNumber.Two, 300);

        // Assert
        Assert.Equal(made, freeThrowEvent.Made);
    }

    [Fact]
    public void FoulEvent_WithValidData_Succeeds()
    {
        // Act
        var foulEvent = new FoulEvent("team-1", "player-1", FoulType.Personal, "player-2", false, PeriodNumber.One, 200);

        // Assert
        Assert.NotNull(foulEvent.Id);
        Assert.Equal(EventType.Foul, foulEvent.Type);
        Assert.Equal(FoulType.Personal, foulEvent.FoulType);
        Assert.Equal("player-2", foulEvent.PlayerFouledId);
        Assert.False(foulEvent.Flagrant);
    }

    [Fact]
    public void FoulEvent_WithFlagrantFoul_Succeeds()
    {
        // Act
        var foulEvent = new FoulEvent("team-1", "player-1", FoulType.Flagrant, "player-2", true, PeriodNumber.One, 200);

        // Assert
        Assert.Equal(FoulType.Flagrant, foulEvent.FoulType);
        Assert.True(foulEvent.Flagrant);
    }

    [Fact]
    public void FoulEvent_WithEmptyFouledPlayerId_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new FoulEvent("team-1", "player-1", FoulType.Personal, "", false, PeriodNumber.One, 200));
    }

    [Fact]
    public void SubstitutionEvent_WithValidData_Succeeds()
    {
        // Act
        var subEvent = new SubstitutionEvent("team-1", "player-in", "player-out", PeriodNumber.Two, 300);

        // Assert
        Assert.NotNull(subEvent.Id);
        Assert.Equal(EventType.Substitution, subEvent.Type);
        Assert.Equal("player-in", subEvent.PlayerId);
        Assert.Equal("player-out", subEvent.PlayerOutId);
    }

    [Fact]
    public void SubstitutionEvent_WithEmptyPlayerOutId_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new SubstitutionEvent("team-1", "player-in", "", PeriodNumber.One, 100));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Event_WithEmptyTeamId_Throws(string teamId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new FreeThrowEvent(teamId, "player-1", true, FoulType.Personal, PeriodNumber.One, 100));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Event_WithEmptyPlayerId_Throws(string playerId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new FreeThrowEvent("team-1", playerId, true, FoulType.Personal, PeriodNumber.One, 100));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(601)]
    public void Event_WithInvalidPeriodTimestamp_Throws(int timestamp)
    {
        // Arrange
        var coordinates = new Coordinates(50, 50);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new ScoreEvent("team-1", "player-1", 2, coordinates, PeriodNumber.One, timestamp));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(300)]
    [InlineData(600)]
    public void Event_WithValidPeriodTimestamp_Succeeds(int timestamp)
    {
        // Arrange
        var coordinates = new Coordinates(50, 50);

        // Act
        var scoreEvent = new ScoreEvent("team-1", "player-1", 2, coordinates, PeriodNumber.One, timestamp);

        // Assert
        Assert.Equal(timestamp, scoreEvent.PeriodTimestamp);
    }
}
