using System.Diagnostics;

namespace OpenGLEngine.ECS;

internal class EntityManager
{
    private readonly int _maxEntities;
    private readonly Queue<Entity> _availableEntities;
        
    private int _livingEntityCount;
    private readonly Signature[] _signatures;

    public EntityManager(int maxEntities, EntitySystem entitySystem)
    {
        _maxEntities = maxEntities;
        _livingEntityCount = 0;
        _availableEntities = new Queue<Entity>(maxEntities);
        _signatures = new Signature[maxEntities];

        for (Entity i = new Entity(entitySystem); i < maxEntities; i++)
        {
            _availableEntities.Enqueue(i);
        }
    }

    public Entity CreateEntity()
    {
        Debug.Assert(_livingEntityCount < _maxEntities, "Too many entities in existence");

        var entity = _availableEntities.Dequeue();
        ++_livingEntityCount;

        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        Debug.Assert(entity < _maxEntities, "Entity out of range");

        _signatures[entity].Reset();
        _availableEntities.Enqueue(entity);
        --_livingEntityCount;
    }

    public void SetSignature(Entity entity, Signature signature)
    {
        Debug.Assert(entity < _maxEntities, "Entity out of range");

        _signatures[entity] = signature;
    }

    public ref Signature GetSignature(Entity entity)
    {
        Debug.Assert(entity < _maxEntities, "Entity out of range");

        return ref _signatures[entity];
    }
}