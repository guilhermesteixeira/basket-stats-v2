namespace BasketStats.Domain.ValueObjects;

using Abstractions;

public class Coordinates : ValueObject
{
    public decimal X { get; }
    public decimal Y { get; }

    public Coordinates(decimal x, decimal y)
    {
        if (x < 0 || x > 100)
            throw new ArgumentException("X coordinate must be between 0 and 100", nameof(x));

        if (y < 0 || y > 100)
            throw new ArgumentException("Y coordinate must be between 0 and 100", nameof(y));

        X = x;
        Y = y;
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return X;
        yield return Y;
    }
}
