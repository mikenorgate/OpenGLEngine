using OpenTK.Mathematics;

namespace OpenGLEngine.Components;

internal struct TransformComponent
{
    public Vector3 Translation { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public TransformComponent(Vector3 translation, Vector3 rotation, Vector3 scale)
    {
        Translation = translation;
        Rotation = rotation;
        Scale = scale;
    }

    public Matrix4 GetTransform()
    {
        var rotation = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rotation));

        return Matrix4.CreateTranslation(Translation) * rotation * Matrix4.CreateScale(Scale);
    }
}