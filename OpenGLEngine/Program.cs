using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenGLEngine.Components;
using OpenGLEngine.ECS;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace OpenGLEngine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var settings = GameWindowSettings.Default;
            settings.RenderFrequency = 0;
            settings.UpdateFrequency = 0;
            using (var window = new Window(settings))
            {
                window.Run();
            }
        }
    }

    internal class Window : GameWindow
    {
        private EntitySystem _es;
        private Shader? _shader;
        //private Texture? _texture;
        private readonly RenderSystem _renderSystem;
        private readonly CameraSystem _cameraSystem;
        private readonly ScriptSystem _scriptSystem;

        public Window(GameWindowSettings gameWindowSettings) : base(gameWindowSettings, new NativeWindowSettings())
        {

            _es = new EntitySystem(100);
            _es.RegisterComponent<MeshComponent>();
            _es.RegisterComponent<CameraComponent>();
            _es.RegisterComponent<TransformComponent>();
            _es.RegisterComponent<ScriptComponent>();

            _renderSystem = _es.RegisterSystem<RenderSystem>();
            _cameraSystem = _es.RegisterSystem<CameraSystem>();
            _scriptSystem = _es.RegisterSystem<ScriptSystem>();

            var signature = new Signature();
            signature.Set(_es.GetComponentType<MeshComponent>());
            _es.SetSystemSignature<RenderSystem>(signature);

            signature = new Signature();
            signature.Set(_es.GetComponentType<CameraComponent>());
            signature.Set(_es.GetComponentType<TransformComponent>());
            _es.SetSystemSignature<CameraSystem>(signature);

            signature = new Signature();
            signature.Set(_es.GetComponentType<ScriptComponent>());
            _es.SetSystemSignature<ScriptSystem>(signature);


            var camera = _es.CreateEntity();
            _es.AddComponent(camera, new CameraComponent(CameraProjectionType.Perspective, 0.1f, 100.0f, MathHelper.DegreesToRadians(45.0f), 0, (float)Size.X / Size.Y));
            _es.AddComponent(camera, new TransformComponent(new Vector3(0.0f, 0.0f, 3.0f), Vector3.Zero, Vector3.One));
            _es.AddComponent(camera, new ScriptComponent(CameraScript));

            Size = (1920, 1080);
            VSync = VSyncMode.Off;
        }


        private void CameraScript(Entity entity, float timeDelta)
        {
            if (!IsFocused)
                return;


            ref var cameraComponent = ref entity.GetComponent<CameraComponent>();

            var input = KeyboardState;

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;


            if (input.IsKeyDown(Keys.W))
            {
                cameraComponent.Position += cameraComponent.Front * cameraSpeed * timeDelta;
            }
            if (input.IsKeyDown(Keys.S))
            {
                cameraComponent.Position -= cameraComponent.Front * cameraSpeed * timeDelta;
            }
            if (input.IsKeyDown(Keys.A))
            {
                cameraComponent.Position -= Vector3.Normalize(Vector3.Cross(cameraComponent.Front, cameraComponent.Up)) * cameraSpeed * timeDelta;
            }
            if (input.IsKeyDown(Keys.D))
            {
                cameraComponent.Position += Vector3.Normalize(Vector3.Cross(cameraComponent.Front, cameraComponent.Up)) * cameraSpeed * timeDelta;
            }

            var mouse = MouseState;

            var deltaX = mouse.X - mouse.PreviousX;
            var deltaY = mouse.Y - mouse.PreviousY;

            cameraComponent.Yaw += deltaX * sensitivity;
            cameraComponent.Pitch -= deltaY * sensitivity;

            if (cameraComponent.Pitch > 89.0f)
                cameraComponent.Pitch = 89.0f;
            if (cameraComponent.Pitch < -89.0f)
                cameraComponent.Pitch = -89.0f;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("shader.vert", "shader.frag");
            //_texture = new Texture("container.jpg");



            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            //var cubeModel = ModelLoader.CreateMeshFromModel("resources\\models\\cube.obj");
            var cubeModel = ModelLoader.CreateMeshFromModel("resources\\models\\backpack\\backpack.obj");


            var cube = _es.CreateEntity();
            _es.AddComponent(cube, cubeModel);




            //var cube2 = _es.CreateEntity();
            //_es.AddComponent(ref cube2, ref planeComponent);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            _scriptSystem.OnUpdate((float)args.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader!.Use();


            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55.0f));


            GL.UniformMatrix4(GL.GetUniformLocation(_shader.Handle, "model"), true, ref model);

            _cameraSystem.Draw(_shader);
            _renderSystem.Draw(_shader);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            _cameraSystem.OnResize(e.Size);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _shader!.Dispose();
        }
    }

    internal class RenderSystem : ECS.System
    {
        public void Draw(Shader shader)
        {
            var i = 0;
            foreach (var entity in Entities)
            {
                var meshComponent = entity.GetComponent<MeshComponent>();

                var translation = new Vector3(0.0f, 0.0f, 0f);
                //var rotation = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(GLFW.GetTime() * 10));
                //rotation *= Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(GLFW.GetTime() * 10));
                ////var rotation = new Vector3((float)GLFW.GetTime() * (i), 0f, (float)GLFW.GetTime());
                var scale = new Vector3(0.25f, 0.25f, 0.25f);

                //var r = rotation;
                var r = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Vector3.Zero));
                var t = Matrix4.CreateTranslation(translation);
                var s = Matrix4.CreateScale(scale);

                var transform = t * r * s;

                GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "transform"), true, ref transform);

                meshComponent.Draw(shader);
                i++;
            }
        }
    }

    internal class CameraSystem : ECS.System
    {
        public void Draw(Shader shader)
        {
            var cameraEntity = Entities.First(); //TODO: what if more than one camera

            var cameraComponent = cameraEntity.GetComponent<CameraComponent>();

            var projection = cameraComponent.GetProjection();

            var transformComponent = cameraEntity.GetComponent<TransformComponent>();

            var view = cameraComponent.GetView() * transformComponent.GetTransform().Inverted();

            GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "projection"), true, ref projection);
        }

        public void OnResize(Vector2i size)
        {
            var cameraEntity = Entities.First(); //TODO: what if more than one camera

            var cameraComponent = cameraEntity.GetComponent<CameraComponent>();

            cameraComponent.SetViewportSize(size.X, size.Y);
        }
    }

    internal class ScriptSystem : ECS.System
    {
        public void OnUpdate(float timeDelta)
        {
            foreach (var entity in Entities)
            {
                var component = entity.GetComponent<ScriptComponent>();

                component.OnUpdate(entity, timeDelta);
            }
        }
    }

    internal class Texture
    {
        public int Handle { get; }

        public Texture(string path)
        {
            Handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            // stb_image loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            // This will correct that, making the texture display properly.
            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult image;
            using (var fs = File.OpenRead(path))
                image = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 5)]
    struct Vertex
    {
        public Vertex(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            TexCoords = texCoords;
        }

        public Vector3 Position { get; }
        public Vector2 TexCoords { get; }
    }














    internal class Shader : IDisposable
    {
        public int Handle { get; }

        public Shader(string vertexPath, string fragmentPath)
        {
            var vertexShaderSource = File.ReadAllText(vertexPath);

            var fragmentShaderSource = File.ReadAllText(fragmentPath);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);

            GL.CompileShader(vertexShader);

            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var success);
            if (success == 0)
            {
                var infoLog = GL.GetShaderInfoLog(vertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(fragmentShader);

            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                var infoLog = GL.GetShaderInfoLog(fragmentShader);
                Console.WriteLine(infoLog);
            }

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                var infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        private void ReleaseUnmanagedResources()
        {
            GL.DeleteProgram(Handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Shader()
        {
            Dispose(false);
        }
    }

}