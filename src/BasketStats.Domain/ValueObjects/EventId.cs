namespace BasketStats.Domain.ValueObjects;

using Abstractions;

public class EventId : ValueObject
{
    public string Value { get; }

    public EventId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Event ID cannot be empty", nameof(value));

        Value = value;
    }

    public static EventId CreateNew() => new(Guid.NewGuid().ToString());

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
