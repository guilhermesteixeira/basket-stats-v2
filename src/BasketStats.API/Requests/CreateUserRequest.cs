namespace BasketStats.API.Requests;

public record CreateUserRequest(string Email, string Name, string KeycloakId);
