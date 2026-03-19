namespace BasketStats.Domain.Abstractions;

using Entities;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task SaveAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
