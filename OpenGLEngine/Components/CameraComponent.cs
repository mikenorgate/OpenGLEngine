using OpenTK.Mathematics;

namespace OpenGLEngine.Components;

internal struct CameraComponent
{
    private Matrix4 _projection;
    private CameraProjectionType _projectionType = CameraProjectionType.Orthographic;
    private float _perspectiveFov = MathHelper.PiOver2;
    private float _nearClip = -1.0f;
    private float _farClip = 1.0f;
    private float _orthographicSize = 10;
    private float _pitch = 0;
    private float _yaw = -MathHelper.PiOver2;


    public CameraProjectionType ProjectionType
    {
        get => _projectionType;
        set
        {
            _projectionType = value;
            RecalcuateProjection();
        }
    }

    public float PerspectiveFOV
    {
        get => MathHelper.RadiansToDegrees(_perspectiveFov);
        set
        {
            _perspectiveFov = MathHelper.DegreesToRadians(value);
            RecalcuateProjection();
        }
    }

    public float NearClip
    {
        get => _nearClip;
        set
        {
            _nearClip = value;
            RecalcuateProjection();
        }
    }

    public float FarClip
    {
        get => _farClip;
        set
        {
            _farClip = value;
            RecalcuateProjection();
        }
    }

    public float OrthographicSize
    {
        get => _orthographicSize;
        set
        {
            _orthographicSize = value;
            RecalcuateProjection();
        }
    }

    public float AspectRatio { get; private set; } = 0;
    public Vector3 Position { get; set; } = Vector3.Zero;

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            _pitch = MathHelper.DegreesToRadians(value);
            RecalculateView();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            RecalculateView();
        }
    }

    public Vector3 Front { get; private set; } = Vector3.Zero;

    public Vector3 Right { get; private set; } = Vector3.Zero;

    public Vector3 Up { get; private set; } = Vector3.Zero;

    public CameraComponent()
    {
        RecalcuateProjection();
        RecalculateView();
    }

    public CameraComponent(CameraProjectionType projectionType, float nearClip, float farClip, float perspectiveFOV,
        float orthographicSize, float aspectRatio)
    {
        _projectionType = projectionType;
        _nearClip = nearClip;
        _farClip = farClip;
        _perspectiveFov = perspectiveFOV;
        _orthographicSize = orthographicSize;
        AspectRatio = aspectRatio;

        RecalcuateProjection();
        RecalculateView();
    }

    public Matrix4 GetProjection()
    {
        return _projection;
    }

    public Matrix4 GetView()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }

    public void SetViewportSize(int width, int height)
    {
        AspectRatio = (float)width / height;
        RecalcuateProjection();
    }

    private void RecalcuateProjection()
    {
        if (_projectionType == CameraProjectionType.Perspective)
        {
            _projection = Matrix4.CreatePerspectiveFieldOfView(_perspectiveFov, AspectRatio, _nearClip, _farClip);
        }
        else
        {
            var orthoLeft = -_orthographicSize * AspectRatio * 0.5f;
            var orthoRight = _orthographicSize * AspectRatio * 0.5f;
            var orthoBottom = -_orthographicSize * 0.5f;
            var orthTop = _orthographicSize * 0.5f;

            _projection =
                Matrix4.CreateOrthographicOffCenter(orthoLeft, orthoRight, orthoBottom, orthTop, _nearClip, _farClip);
        }
    }

    private void RecalculateView()
    {
        Front = Vector3.Normalize(new Vector3(MathF.Cos(_pitch) * MathF.Cos(_yaw), MathF.Sin(_pitch), MathF.Cos(_pitch) * MathF.Sin(_yaw)));
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}