using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private Texture? _texture;
        private readonly RenderSystem _renderSystem;

        public Window(GameWindowSettings gameWindowSettings) : base(gameWindowSettings, new NativeWindowSettings())
        {
            Size = (1920, 1080);
            VSync = VSyncMode.Off;
            _es = new EntitySystem(100);
            _es.RegisterComponent<MeshComponent>();

            _renderSystem = _es.RegisterSystem<RenderSystem>();

            var signature = new Signature();
            signature.Set(_es.GetComponentType<MeshComponent>());
            _es.SetSystemSignature<RenderSystem>(signature);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("shader.vert", "shader.frag");
            _texture = new Texture("container.jpg");



            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);


            var cube = _es.CreateEntity();
            var planeComponent = MeshComponent.Cube(new[] { _texture });
            _es.AddComponent(cube, planeComponent);

            //var cube2 = _es.CreateEntity();
            //_es.AddComponent(ref cube2, ref planeComponent);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader!.Use();


            Matrix4 model = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-30.0f));
            model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-30.0f));

            Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -2.0f);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Size.X / Size.Y, 0.1f, 100.0f);

            GL.UniformMatrix4(GL.GetUniformLocation(_shader.Handle, "model"), true, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shader.Handle, "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shader.Handle, "projection"), true, ref projection);


            _renderSystem.Draw(_shader);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
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

                var translation = new Vector3(0.5f, -0.5f, 0f);
                var rotation = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(GLFW.GetTime() * 10));
                rotation *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(GLFW.GetTime() * 10));
                //var rotation = new Vector3((float)GLFW.GetTime() * (i), 0f, (float)GLFW.GetTime());
                var scale = new Vector3(0.5f, 0.3f, 0f);

                var r = rotation;
                //var r = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
                var t = Matrix4.CreateTranslation(translation);
                var s = Matrix4.CreateScale(scale);

                var transform = t * r * s;

                GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "transform"), true, ref transform);

                meshComponent.Draw(shader);
                i++;
            }
        }
    }

    internal struct MeshComponent
    {
        //TODO: component should have multiple meshes
        private readonly Vertex[] _vertices;
        private readonly uint[] _indices;
        private readonly Texture[] _textures;
        private int _vao;
        private int _vbo;
        private int _ebo;

        public MeshComponent(Vertex[] vertices, uint[] indices, Texture[] textures)
        {
            _vertices = vertices;
            _indices = indices;
            _textures = textures;

            SetupMesh();
        }

        unsafe void SetupMesh()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(Vertex), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Draw(Shader shader)
        {
            for (var i = 0; i < _textures.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.Uniform1(GL.GetUniformLocation(shader.Handle, $"texture{i}"), i);
                GL.BindTexture(TextureTarget.ProxyTexture2D, _textures[i].Handle);
            }

            GL.BindVertexArray(_vao);
            GL.DrawElements(BeginMode.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        private void ReleaseUnmanagedResources()
        {
            GL.DeleteBuffer(_ebo);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            _ebo = 0;
            _vbo = 0;
            _vao = 0;
        }

        private static readonly Vertex[] PlaneVertices = new[]
        {
            new Vertex(new Vector3(0.5f,0.5f,0.0f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(0.5f,-0.5f,0.0f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,0.0f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,0.5f,0.0f), new Vector2(0.0f, 1.0f))
        };

        private static readonly uint[] PlaneIndices = new uint[]
        {
            0, 1, 3,
            1, 2, 3
        };

        private static readonly Vertex[] CubeVertices = new[]
        {
            new Vertex(new Vector3(0.5f,0.5f,-0.5f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(0.5f,-0.5f,-0.5f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,-0.5f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,0.5f,-0.5f), new Vector2(0.0f, 1.0f)),

            new Vertex(new Vector3(0.5f,0.5f,0.5f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(0.5f,-0.5f,0.5f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,0.5f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,0.5f,0.5f), new Vector2(0.0f, 1.0f)),

            new Vertex(new Vector3(-0.5f,0.5f,-0.5f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(-0.5f,0.5f,0.5f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,0.5f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,-0.5f), new Vector2(0.0f, 1.0f)),

            new Vertex(new Vector3(0.5f,0.5f,-0.5f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(0.5f,0.5f,0.5f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(0.5f,-0.5f,0.5f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(0.5f,-0.5f,-0.5f), new Vector2(0.0f, 1.0f)),

            new Vertex(new Vector3(0.5f,-0.5f,-0.5f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(0.5f,-0.5f,0.5f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,0.5f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,-0.5f,-0.5f), new Vector2(0.0f, 1.0f)),

            new Vertex(new Vector3(0.5f,0.5f,-0.5f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(0.5f,0.5f,0.5f), new Vector2(1.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,0.5f,0.5f), new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(-0.5f,0.5f,-0.5f), new Vector2(0.0f, 1.0f)),
        };


        private static readonly uint[] CubeIndices = new uint[]
        {
            0, 1, 3,
            1, 2, 3,

            4, 5, 7,
            5, 6, 7,

            8, 9, 11,
            9, 10, 11,

            12, 13, 15,
            13, 14, 15,

            16, 17, 19,
            17, 18, 19,

            20, 21, 23,
            21, 22, 23
        };

        public static MeshComponent Plane(Texture[] textures) => new MeshComponent(PlaneVertices, PlaneIndices, textures);
        public static MeshComponent Cube(Texture[] textures) => new MeshComponent(CubeVertices, CubeIndices, textures);
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