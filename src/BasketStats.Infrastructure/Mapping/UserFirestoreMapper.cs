using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Models;

namespace BasketStats.Infrastructure.Mapping;

public static class UserFirestoreMapper
{
    public static UserDocument ToDocument(User user)
    {
        return new UserDocument
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            KeycloakId = user.KeycloakId,
            Roles = user.Roles.ToList(),
            CreatedAt = user.CreatedAt
        };
    }

    public static User ToDomain(UserDocument doc)
    {
        var user = User.Create(doc.Id, doc.Email, doc.Name, doc.KeycloakId);
        typeof(User).GetProperty(nameof(User.CreatedAt))!.SetValue(user, doc.CreatedAt);
        foreach (var role in doc.Roles)
            user.AddRole(role);
        return user;
    }
}
