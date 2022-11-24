using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGLEngine.Components
{
    internal struct MeshComponent
    {
        private readonly Mesh[] _meshes;

        internal MeshComponent(Mesh[] meshes)
        {
            _meshes = meshes;
        }

        public void Draw(Shader shader)
        {
            for (var i = 0; i < _meshes.Length; i++)
                _meshes[i].Draw(shader);
        }


        [StructLayout(LayoutKind.Sequential, Size = sizeof(float) * 8)]
        internal struct Vertex
        {
            public Vector3 Position { get; }
            public Vector3 Normal { get; }
            public Vector2 TexCoords { get; }

            public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords)
            {
                Position = position;
                Normal = normal;
                TexCoords = texCoords;
            }
        }

        internal struct Mesh
        {
            private int _vao;
            private int _vbo;
            private int _ebo;

            public Vertex[] Vertices { get; }
            public uint[] Indices { get; }
            public Texture[] Textures { get; }

            public Mesh(Vertex[] vertices, uint[] indices, Texture[] textures)
            {
                Vertices = vertices;
                Indices = indices;
                Textures = textures;

                SetupMesh();
            }

            unsafe void SetupMesh()
            {
                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();
                _ebo = GL.GenBuffer();

                GL.BindVertexArray(_vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(Vertex), Vertices, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);

                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 3 * sizeof(float));

                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), 6 * sizeof(float));

                GL.BindVertexArray(0);
            }

            public void Draw(Shader shader)
            {
                int diffuseNum = 1, heightNum = 1, normalNum = 1, speculareNum = 1;
                for (var i = 0; i < Textures.Length; i++)
                {
                    string name;
                    switch (Textures[i].Type)
                    {
                        case TextureType.Diffuse:
                            name = $"material.texture_diffuse{diffuseNum++}";
                            break;
                        case TextureType.Height:
                            name = $"material.texture_height{heightNum++}";
                            break;
                        case TextureType.Normals:
                            name = $"material.texture_normal{normalNum++}";
                            break;
                        case TextureType.Specular:
                            name = $"material.texture_specular{speculareNum++}";
                            break;

                        default:
                            continue;
                    }

                    var uniformLocation = GL.GetUniformLocation(shader.Handle, name);
                    GL.ActiveTexture(TextureUnit.Texture0 + i);

                    GL.BindTexture(TextureTarget.Texture2D, Textures[i].Id);
                    GL.Uniform1(uniformLocation, i);
                }


                GL.BindVertexArray(_vao);
                GL.DrawElements(BeginMode.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);

                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }
    }

    internal struct Texture
    {
        public int Id { get; }
        public TextureType Type { get; }

        public Texture(int id, TextureType type)
        {
            Id = id;
            Type = type;
        }
    }

    internal enum TextureType
    {
        Diffuse,
        Specular,
        Normals,
        Height
    }
}
