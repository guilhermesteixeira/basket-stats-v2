namespace BasketStats.API.Models;

/// <summary>
/// Represents a user/player
/// </summary>
public class User
{
    public string Id { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string KeycloakId { get; set; } = string.Empty;
    
    public List<string> Roles { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// User roles
/// </summary>
public static class UserRoles
{
    public const string Admin = "admin";
    public const string MatchCreator = "match_creator";
    public const string Viewer = "viewer";
}
