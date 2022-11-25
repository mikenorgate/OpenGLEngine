using System.Diagnostics;

namespace OpenGLEngine.ECS;

internal class ComponentManager
{
    private const int MaxComponents = 32;

    private readonly int _maxEntities;
    private readonly Dictionary<Type, ComponentType> _componentTypes;
    private readonly Dictionary<Type, IComponentArray> _componentArrays;
    private ComponentType _nextComponentType;

    public ComponentManager(int maxEntities)
    {
        _maxEntities = maxEntities;
        _componentTypes = new Dictionary<Type, ComponentType>(MaxComponents);
        _componentArrays = new Dictionary<Type, IComponentArray>(MaxComponents);
        _nextComponentType = new ComponentType();
    }

    public void RegisterComponent<T>()
        where T : struct
    {
        Debug.Assert(!_componentTypes.ContainsKey(typeof(T)), "Cannot register component more than once");
        Debug.Assert(_componentTypes.Count + 1 <= MaxComponents, "Reached component limit");

        _componentTypes[typeof(T)] = _nextComponentType;
        _componentArrays[typeof(T)] = new ComponentArray<T>(_maxEntities);

        ++_nextComponentType;
    }

    public ComponentType GetComponentType<T>()
        where T : struct
    {
        Debug.Assert(_componentTypes.ContainsKey(typeof(T)), "Type not registered");
        
        return _componentTypes[typeof(T)];
    }

    public T GetComponent<T>(Entity entity)
        where T : struct
    {
        return GetComponentArray<T>().GetData(entity);
    }

    public void AddComponent<T>(Entity entity, T component)
        where T : struct
    {
        GetComponentArray<T>().InsertData(entity, component);
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct
    {
        GetComponentArray<T>().RemoveData(entity);
    }

    public void EntityDestroyed(Entity entity)
    {
        foreach (var componentArray in _componentArrays.Values)
        {
            componentArray.EntityDestroyed(entity);
        }
    }
    
    ComponentArray<T> GetComponentArray<T>()
        where T : struct
    {
        Debug.Assert(_componentTypes.ContainsKey(typeof(T)), "Type not registered");

        return (ComponentArray<T>)_componentArrays[typeof(T)];
    }


    interface IComponentArray
    {
        void EntityDestroyed(Entity entity);
    }

    class ComponentArray<T> : IComponentArray
        where T : struct
    {
        private readonly T?[] _componentArray;

        private int _size;

        public ComponentArray(int maxEntities)
        {
            _componentArray = new T?[maxEntities];

            _size = 0;
        }

        public void InsertData(Entity entity, T component)
        {
            Debug.Assert(!_componentArray[entity].HasValue, "Component added to same entity more than once");
            
            _componentArray[entity] = component;
            ++_size;
        }

        public void RemoveData(Entity entity)
        {
            Debug.Assert(_componentArray[entity].HasValue, "Removing non-existant component");

            _componentArray[entity] = null;

            --_size;
        }

        public T GetData(Entity entity)
        {
            Debug.Assert(_componentArray[entity].HasValue, "Retrieving non-existant component");

            return _componentArray[entity]!.Value;
        }

        public void EntityDestroyed(Entity entity)
        {
            if (_componentArray[entity].HasValue)
                RemoveData(entity);
        }
    }
}