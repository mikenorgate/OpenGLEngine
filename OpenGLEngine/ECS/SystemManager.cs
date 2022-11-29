using System.Diagnostics;

namespace OpenGLEngine.ECS;

internal class SystemManager
{
    private readonly Dictionary<Type, System> _systems;
    private readonly Dictionary<Type, Signature> _signatures;

    public SystemManager()
    {
        _systems = new Dictionary<Type, System>();
        _signatures = new Dictionary<Type, Signature>();
    }

    public T RegisterSystem<T>()
        where T : System, new()
    {
        Debug.Assert(!_systems.ContainsKey(typeof(T)), "Cannot register system more than once");

        var system = new T();
        _systems[typeof(T)] = system;
        return system;
    }

    public void SetSignature<T>(Signature signature)
        where T : System
    {
        Debug.Assert(_systems.ContainsKey(typeof(T)), "System used before registered");

        _signatures[typeof(T)] = signature;
    }

    public void EntityDestroyed(Entity entity)
    {
        foreach (var system in _systems.Values)
            system.Entities.Remove(entity);
    }

    public void EntitySignatureChanged(Entity entity, Signature signature)
    {
        foreach (var type in _systems.Keys)
        {
            var system = _systems[type];
            var systemSignature = _signatures[type];

            if ((signature & systemSignature) != 0)
            {
                system.Entities.Add(entity);
            }
            else
            {
                system.Entities.Remove(entity);
            }
        }
    }
}