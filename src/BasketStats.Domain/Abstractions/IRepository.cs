namespace BasketStats.Domain.Abstractions;

public interface IRepository<in TEntity> where TEntity : Entity
{
}
