namespace BasketStats.Domain.Entities;

using Abstractions;

public class Team : Entity
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Team()
    {
    }

    public static Team Create(string id, string name, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Team ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));

        return new Team
        {
            Id = id,
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsOwnedBy(string userId) => OwnerId == userId;
}
