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

    public ref T GetComponent<T>(Entity entity)
        where T : struct
    {
        return ref GetComponentArray<T>().GetData(entity);
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
        private readonly T[] _componentArray;
        private readonly bool[] _existsArray;

        private int _size;

        public ComponentArray(int maxEntities)
        {
            _componentArray = new T[maxEntities];
            _existsArray = new bool[maxEntities];

            _size = 0;
        }

        public void InsertData(Entity entity, T component)
        {
            Debug.Assert(!_existsArray[entity], "Component added to same entity more than once");
            
            _componentArray[entity] = component;
            _existsArray[entity] = true;
            ++_size;
        }

        public void RemoveData(Entity entity)
        {
            Debug.Assert(_existsArray[entity], "Removing non-existant component");

            _existsArray[entity] = false;

            --_size;
        }

        public ref T GetData(Entity entity)
        {
            Debug.Assert(_existsArray[entity], "Retrieving non-existant component");

            return ref _componentArray[entity];
        }

        public void EntityDestroyed(Entity entity)
        {
            if (_existsArray[entity])
                RemoveData(entity);
        }
    }
}