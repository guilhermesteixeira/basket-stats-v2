namespace BasketStats.Domain.ValueObjects;

using Abstractions;

public class MatchId : ValueObject
{
    public string Value { get; }

    public MatchId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Match ID cannot be empty", nameof(value));

        Value = value;
    }

    public static MatchId CreateNew() => new(Guid.NewGuid().ToString());

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
