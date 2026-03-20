using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Models;

namespace BasketStats.Infrastructure.Mapping;

public static class TeamFirestoreMapper
{
    public static TeamDocument ToDocument(Team team)
    {
        return new TeamDocument
        {
            Id = team.Id,
            Name = team.Name,
            OwnerId = team.OwnerId,
            CreatedAt = team.CreatedAt
        };
    }

    public static Team ToDomain(TeamDocument doc)
    {
        var team = Team.Create(doc.Id, doc.Name, doc.OwnerId);
        typeof(Team).GetProperty(nameof(Team.CreatedAt))!.SetValue(team, doc.CreatedAt);
        return team;
    }
}
