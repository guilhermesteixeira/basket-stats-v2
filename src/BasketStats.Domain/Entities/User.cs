namespace BasketStats.Domain.Entities;

using Abstractions;

public class User : Entity
{
    public string Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public string KeycloakId { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(string id, string email, string name, string keycloakId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("User ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new ArgumentException("Keycloak ID cannot be empty", nameof(keycloakId));

        return new User
        {
            Id = id,
            Email = email,
            Name = name,
            KeycloakId = keycloakId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty", nameof(role));

        if (!Roles.Contains(role))
            Roles.Add(role);
    }

    public bool HasRole(string role) => Roles.Contains(role);

    public bool IsMatchCreator => HasRole("match-creator");

    public bool IsAdmin => HasRole("admin");
}
