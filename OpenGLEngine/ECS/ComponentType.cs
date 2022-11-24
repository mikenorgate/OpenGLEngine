namespace OpenGLEngine.ECS;

public readonly struct ComponentType : IEquatable<ComponentType>
{
    public int Id { get; }

    public ComponentType()
    {
        Id = 1;
    }

    ComponentType(int id)
    {
        Id = id;
    }

    public bool Equals(ComponentType other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is ComponentType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(ComponentType left, ComponentType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ComponentType left, ComponentType right)
    {
        return !left.Equals(right);
    }

    public static ComponentType operator ++(ComponentType a)
    {
        return new ComponentType(a.Id + 1);
    }
}