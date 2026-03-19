namespace BasketStats.Domain.Tests.ValueObjectTests;

using Domain.ValueObjects;

public class PeriodTests
{
    [Fact]
    public void Create_WithValidPeriod_Succeeds()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var period = new Period(PeriodNumber.One, startTime);

        // Assert
        Assert.Equal(PeriodNumber.One, period.Number);
        Assert.Equal(startTime, period.StartTime);
        Assert.Null(period.EndTime);
        Assert.True(period.IsActive);
    }

    [Fact]
    public void End_WithValidEndTime_Succeeds()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var period = new Period(PeriodNumber.One, startTime);
        var endTime = startTime.AddSeconds(300);

        // Act
        period.End(endTime);

        // Assert
        Assert.Equal(endTime, period.EndTime);
        Assert.False(period.IsActive);
    }

    [Fact]
    public void End_WithEndTimeBeforeStartTime_Throws()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var period = new Period(PeriodNumber.One, startTime);
        var endTime = startTime.AddSeconds(-100);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => period.End(endTime));
    }

    [Fact]
    public void End_WithEndTimeSameAsStartTime_Throws()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var period = new Period(PeriodNumber.One, startTime);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => period.End(startTime));
    }

    [Fact]
    public void ElapsedSeconds_WithActivePeriod_ReturnsElapsedTime()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddSeconds(-100);
        var period = new Period(PeriodNumber.One, startTime);

        // Act
        var elapsed = period.ElapsedSeconds;

        // Assert
        Assert.True(elapsed > 0);
        Assert.True(elapsed <= Period.DurationSeconds);
    }

    [Fact]
    public void ElapsedSeconds_WithFinishedPeriod_ReturnsExactElapsedTime()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var period = new Period(PeriodNumber.One, startTime);
        var endTime = startTime.AddSeconds(250);
        period.End(endTime);

        // Act
        var elapsed = period.ElapsedSeconds;

        // Assert
        Assert.Equal(250, elapsed);
    }

    [Fact]
    public void ElapsedSeconds_WithFinishedPeriod_CapsAtDurationSeconds()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var period = new Period(PeriodNumber.One, startTime);
        var endTime = startTime.AddSeconds(700); // More than 600 seconds
        period.End(endTime);

        // Act
        var elapsed = period.ElapsedSeconds;

        // Assert
        Assert.Equal(Period.DurationSeconds, elapsed);
    }

    [Theory]
    [InlineData(PeriodNumber.One)]
    [InlineData(PeriodNumber.Two)]
    [InlineData(PeriodNumber.Three)]
    [InlineData(PeriodNumber.Four)]
    public void AllPeriodNumbers_AreValid(PeriodNumber periodNumber)
    {
        // Arrange & Act
        var period = new Period(periodNumber, DateTime.UtcNow);

        // Assert
        Assert.Equal(periodNumber, period.Number);
    }
}
