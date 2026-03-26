namespace BasketStats.Domain.Abstractions;

using Entities;

public interface ITeamRepository : IRepository<Team>
{
    Task<Team?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Team>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Team>> GetByOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
    Task SaveAsync(Team team, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
