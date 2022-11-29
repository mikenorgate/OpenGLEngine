using OpenGLEngine.ECS;

namespace OpenGLEngine.Components;

public struct ScriptComponent
{
    private readonly Action<Entity, float> _script;

    public ScriptComponent(Action<Entity, float> script)
    {
        _script = script;
    }

    public void OnUpdate(Entity entity, float timeDelta)
    {
        _script(entity, timeDelta);
    }
}