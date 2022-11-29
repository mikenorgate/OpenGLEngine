using System.Collections.Specialized;

namespace OpenGLEngine.ECS;

public struct Signature : IEquatable<Signature>, IEquatable<int>
{
    private BitVector32 _signature;

    public Signature()
    {
        _signature = new BitVector32(0);
    }

    Signature(int signature)
    {
        _signature = new BitVector32(signature);
    }

    public void Set(ComponentType componentType, bool value = true)
    {
        _signature[1 << componentType.Id] = value;
    }

    public void Reset()
    {
        _signature = new BitVector32(0);
    }

    public static Signature operator &(Signature a, Signature b)
    {
        return new Signature(a._signature.Data & b._signature.Data);
    }

    public bool Equals(Signature other)
    {
        return _signature.Equals(other._signature);
    }

    public bool Equals(int other)
    {
        return _signature.Data.Equals(other);
    }

    public override bool Equals(object? obj)
    {
        return (obj is Signature other && Equals(other)) || (obj is int o && Equals(o));
    }

    public override int GetHashCode()
    {
        return _signature.GetHashCode();
    }

    public static bool operator ==(Signature left, Signature right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Signature left, Signature right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(Signature left, int right)
    {
        return left._signature.Data.Equals(right);
    }

    public static bool operator !=(Signature left, int right)
    {
        return !left.Equals(right);
    }
}