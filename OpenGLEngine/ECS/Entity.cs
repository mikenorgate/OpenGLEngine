namespace OpenGLEngine.ECS;

public readonly struct Entity : IEquatable<Entity>, IEquatable<int>, IComparable<Entity>, IComparable<int>
{
    private readonly EntitySystem _entitySystem;
    public int Id { get; }

    internal Entity(EntitySystem entitySystem)
    {
        _entitySystem = entitySystem;
        Id = 0;
    }

    Entity(int id, EntitySystem entitySystem)
    {
        _entitySystem = entitySystem;
        Id = id;
    }

    public T GetComponent<T>()
        where T : struct
    {
        return _entitySystem.GetComponent<T>(this);
    }

    public bool Equals(Entity other)
    {
        return Id == other.Id;
    }

    public bool Equals(int other)
    {
        return Id == other;
    }

    public int CompareTo(Entity other)
    {
        return Id.CompareTo(other.Id);
    }

    public int CompareTo(int other)
    {
        return Id.CompareTo(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(Entity left, int right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, int right)
    {
        return !left.Equals(right);
    }

    public static Entity operator ++(Entity a)
    {
        return new Entity(a.Id + 1, a._entitySystem);
    }

    public static Entity operator --(Entity a)
    {
        return new Entity(a.Id - 1, a._entitySystem);
    }

    public static bool operator <(Entity left, Entity right)
    {
        return left.Id < right.Id;
    }

    public static bool operator >(Entity left, Entity right)
    {
        return left.Id < right.Id;
    }

    public static bool operator <(Entity left, int right)
    {
        return left.Id < right;
    }

    public static bool operator >(Entity left, int right)
    {
        return left.Id < right;
    }

    public static implicit operator int(Entity a) => a.Id;
}