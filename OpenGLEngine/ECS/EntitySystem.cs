namespace OpenGLEngine.ECS
{
    /**
     * https://austinmorlan.com/posts/entity_component_system/
     */
    public class EntitySystem
    {
        private readonly SystemManager _systemManager;
        private readonly ComponentManager _componentManager;
        private readonly EntityManager _entityManager;

        public EntitySystem(int maxEntities)
        {
            _systemManager = new SystemManager();
            _componentManager = new ComponentManager(maxEntities);
            _entityManager = new EntityManager(maxEntities, this);
        }

        public Entity CreateEntity()
        {
            return _entityManager.CreateEntity();
        }

        public void DestroyEntity(Entity entity)
        {
            _entityManager.DestroyEntity(entity);

            _componentManager.EntityDestroyed(entity);

            _systemManager.EntityDestroyed(entity);
        }

        public void RegisterComponent<T>()
            where T : struct
        {
            _componentManager.RegisterComponent<T>();
        }

        public void AddComponent<T>(Entity entity, T component)
            where T : struct
        {
            _componentManager.AddComponent(entity, component);

            var signature = _entityManager.GetSignature(entity);
            signature.Set(_componentManager.GetComponentType<T>());
            _entityManager.SetSignature(entity, signature);

            _systemManager.EntitySignatureChanged(entity, signature);
        }

        public void RemoveComponent<T>(Entity entity)
            where T : struct
        {
            _componentManager.RemoveComponent<T>(entity);

            var signature = _entityManager.GetSignature(entity);
            signature.Set(_componentManager.GetComponentType<T>(), false);
            _entityManager.SetSignature(entity, signature);

            _systemManager.EntitySignatureChanged(entity, signature);
        }

        public T GetComponent<T>(Entity entity)
            where T : struct
        {
            return _componentManager.GetComponent<T>(entity);
        }

        public ComponentType GetComponentType<T>()
            where T : struct
        {
            return _componentManager.GetComponentType<T>();
        }

        public T RegisterSystem<T>()
            where T : System, new()
        {
            return _systemManager.RegisterSystem<T>();
        }

        public void SetSystemSignature<T>(Signature signature)
            where T : System
        {
            _systemManager.SetSignature<T>(signature);
        }
    }
}
