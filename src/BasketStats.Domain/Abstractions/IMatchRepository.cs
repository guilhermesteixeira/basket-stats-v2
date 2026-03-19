namespace BasketStats.Domain.Abstractions;

using Entities;

public interface IMatchRepository : IRepository<Match>
{
    Task<Match?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Match>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Match>> GetByTeamAsync(string teamId, CancellationToken cancellationToken = default);
    Task SaveAsync(Match match, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
