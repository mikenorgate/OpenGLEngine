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
        private readonly int[] _entityToIndex;
        private readonly int[] _indexToEntity;

        private int _size;

        public ComponentArray(int maxEntities)
        {
            _componentArray = new T[maxEntities];
            _entityToIndex = new int[maxEntities];
            _indexToEntity = new int[maxEntities];

            Array.Fill(_entityToIndex, -1);
            Array.Fill(_indexToEntity, -1);

            _size = 0;
        }

        public void InsertData(Entity entity, T component)
        {
            Debug.Assert(_entityToIndex[entity] == -1, "Component added to same entity more than once");

            var index = _size;
            _entityToIndex[entity] = index;
            _indexToEntity[index] = entity;
            _componentArray[index] = component;
            ++_size;
        }

        public void RemoveData(Entity entity)
        {
            Debug.Assert(_entityToIndex[entity] >= 0, "Removing non-existant component");

            var indexToRemove = _entityToIndex[entity];
            var indexOfLast = _size - 1;
            _componentArray[indexToRemove] = _componentArray[indexOfLast];

            var entityOfLast = _indexToEntity[indexOfLast];
            _entityToIndex[entityOfLast] = indexToRemove;
            _indexToEntity[indexToRemove] = entityOfLast;

            _entityToIndex[entity] = -1;
            _indexToEntity[indexOfLast] = -1;

            --_size;
        }

        public ref T GetData(Entity entity)
        {
            Debug.Assert(_entityToIndex[entity] >= 0, "Retrieving non-existant component");

            return ref _componentArray[_entityToIndex[entity]];
        }

        public void EntityDestroyed(Entity entity)
        {
            if (_entityToIndex[entity] >= 0)
                RemoveData(entity);
        }
    }
}