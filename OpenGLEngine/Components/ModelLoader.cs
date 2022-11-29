using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace OpenGLEngine.Components;

internal static class ModelLoader
{
    private static readonly Dictionary<string, Texture> LoadedTextures = new(StringComparer.OrdinalIgnoreCase);

    public static MeshComponent CreateMeshFromModel(string path)
    {
        using var assimpContext = new AssimpContext();
        var scene = assimpContext.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
        if (scene == null || (scene.SceneFlags & SceneFlags.Incomplete) == SceneFlags.Incomplete ||
            scene.RootNode == null)
            throw new InvalidOperationException("Failed import");

        var meshes = new List<MeshComponent.Mesh>();

        var workingDir = Path.GetDirectoryName(path);

        ProcessNode(scene.RootNode, scene, meshes, workingDir!);

        return new MeshComponent(meshes.ToArray());
    }

    private static void ProcessNode(Node node, Scene scene, List<MeshComponent.Mesh> meshes, string workingDir)
    {
        foreach (var i in node.MeshIndices)
        {
            var mesh = scene.Meshes[i];
            meshes.Add(ProcessMesh(mesh, scene, workingDir));
        }

        for (var i = 0; i < node.ChildCount; i++) ProcessNode(node.Children[i], scene, meshes, workingDir);
    }

    private static MeshComponent.Mesh ProcessMesh(Mesh mesh, Scene scene, string workingDir)
    {
        var vertices = new MeshComponent.Vertex[mesh.VertexCount];
        var indices = mesh.GetUnsignedIndices();

        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var vertex = mesh.Vertices[i];

            var normal = Vector3.Zero;
            if (mesh.HasNormals)
            {
                var n = mesh.Normals[i];
                normal = new Vector3(n.X, n.Y, n.Z);
            }

            var texCoords = Vector2.Zero;
            if (mesh.HasTextureCoords(0))
            {
                var coords = mesh.TextureCoordinateChannels[0][i];
                texCoords = new Vector2(coords.X, coords.Y);
            }

            vertices[i] = new MeshComponent.Vertex(new Vector3(vertex.X, vertex.Y, vertex.Z), normal, texCoords);
        }

        var textures = new List<Texture>();
        if (mesh.MaterialIndex >= 0)
        {
            var material = scene.Materials[mesh.MaterialIndex];
            var diffuseMaps =
                LoadMaterialTextures(material, Assimp.TextureType.Diffuse, TextureType.Diffuse, workingDir);
            var specularMaps =
                LoadMaterialTextures(material, Assimp.TextureType.Specular, TextureType.Specular, workingDir);
            var normalMaps =
                LoadMaterialTextures(material, Assimp.TextureType.Normals, TextureType.Normals, workingDir);
            var heightMaps = LoadMaterialTextures(material, Assimp.TextureType.Height, TextureType.Height, workingDir);
            textures.AddRange(diffuseMaps);
            textures.AddRange(specularMaps);
            textures.AddRange(normalMaps);
            textures.AddRange(heightMaps);
        }

        return new MeshComponent.Mesh(vertices, indices, textures.ToArray());
    }

    private static Texture[] LoadMaterialTextures(Material material, Assimp.TextureType type, TextureType textureType,
        string workingDir)
    {
        var textures = new List<Texture>();
        var index = 0;
        foreach (var materialTexture in material.GetMaterialTextures(type))
        {
            var path = Path.Combine(workingDir, materialTexture.FilePath);
            if (LoadedTextures.TryGetValue(path, out var texture))
            {
                textures.Add(texture);
            }
            else
            {
                var id = LoadTextureFromFile(path);
                texture = new Texture(id, textureType, index++);
                textures.Add(texture);
                LoadedTextures.Add(path, texture);
            }
        }

        return textures.ToArray();
    }

    private static int LoadTextureFromFile(string path)
    {
        var textureId = GL.GenTexture();

        try
        {
            using var fs = File.OpenRead(path);
            var image = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);

            PixelInternalFormat format;
            PixelFormat pixelFormat;
            switch (image.Comp)
            {
                case ColorComponents.RedGreenBlue:
                    format = PixelInternalFormat.Rgb;
                    pixelFormat = PixelFormat.Rgb;
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    format = PixelInternalFormat.Rgba;
                    pixelFormat = PixelFormat.Rgba;
                    break;
                default:
                    throw new NotSupportedException();
            }

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, format, image.Width, image.Height, 0, pixelFormat,
                PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
        catch
        {
            GL.DeleteTexture(textureId);
            throw;
        }

        return textureId;
    }
}