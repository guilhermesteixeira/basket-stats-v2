namespace BasketStats.Domain.Abstractions;

public abstract class Entity
{
    protected Entity()
    {
    }

    public virtual bool Equals(Entity? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        var entity = obj as Entity;
        return Equals(entity);
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !left.Equals(right);
    }
}
