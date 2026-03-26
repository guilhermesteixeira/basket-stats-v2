namespace BasketStats.Domain.ValueObjects;

public class PlayerInfo
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public int Number { get; private set; }

    private PlayerInfo() { }

    public static PlayerInfo Create(string name, int number)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty", nameof(name));
        if (number < 0 || number > 99)
            throw new ArgumentException("Jersey number must be between 0 and 99", nameof(number));

        return new PlayerInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Number = number
        };
    }

    // For reconstruction from persistence
    public static PlayerInfo Restore(string id, string name, int number)
    {
        return new PlayerInfo { Id = id, Name = name, Number = number };
    }
}
