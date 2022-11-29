namespace OpenGLEngine.Components;

internal readonly struct Texture
{
    public int Id { get; }
    public TextureType Type { get; }
    public string Name { get; }

    public Texture(int id, TextureType type, int index)
    {
        Id = id;
        Type = type;
        Name = $"material.texture_{type.ToString().ToLower()}{index}";
    }
}