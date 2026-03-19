namespace BasketStats.Domain.ValueObjects;

using Abstractions;

public enum PeriodNumber
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4
}

public class Period : ValueObject
{
    public const int DurationSeconds = 600; // 10 minutes FIBA

    public PeriodNumber Number { get; }
    public DateTime StartTime { get; }
    public DateTime? EndTime { get; private set; }

    public Period(PeriodNumber number, DateTime startTime)
    {
        Number = number;
        StartTime = startTime;
    }

    public void End(DateTime endTime)
    {
        if (endTime <= StartTime)
            throw new InvalidOperationException("End time must be after start time");

        EndTime = endTime;
    }

    public bool IsActive => EndTime is null;

    public int ElapsedSeconds
    {
        get
        {
            var elapsed = EndTime.HasValue 
                ? (int)(EndTime.Value - StartTime).TotalSeconds
                : (int)(DateTime.UtcNow - StartTime).TotalSeconds;
            
            return Math.Min(elapsed, DurationSeconds);
        }
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
        yield return StartTime;
        yield return EndTime ?? DateTime.MinValue;
    }
}
