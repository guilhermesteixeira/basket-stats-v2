using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Mapping;

namespace BasketStats.Infrastructure.Tests.Mapping;

public class TeamFirestoreMapperTests
{
    [Fact]
    public void ToDocument_Team_MapsAllProperties()
    {
        // Arrange
        var team = Team.Create("team-1", "Lakers", "owner-1");

        // Act
        var doc = TeamFirestoreMapper.ToDocument(team);

        // Assert
        Assert.Equal(team.Id, doc.Id);
        Assert.Equal(team.Name, doc.Name);
        Assert.Equal(team.OwnerId, doc.OwnerId);
        Assert.Equal(team.CreatedAt, doc.CreatedAt);
    }

    [Fact]
    public void ToDomain_Document_ReconstructsTeamCorrectly()
    {
        // Arrange
        var team = Team.Create("team-1", "Lakers", "owner-1");
        var doc = TeamFirestoreMapper.ToDocument(team);

        // Act
        var result = TeamFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(doc.Id, result.Id);
        Assert.Equal(doc.Name, result.Name);
        Assert.Equal(doc.OwnerId, result.OwnerId);
        Assert.Equal(doc.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public void ToDomain_Document_RoundTrip_PreservesAllData()
    {
        // Arrange
        var original = Team.Create("team-42", "Celtics", "owner-99");
        var doc = TeamFirestoreMapper.ToDocument(original);

        // Act
        var result = TeamFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.OwnerId, result.OwnerId);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
    }
}
