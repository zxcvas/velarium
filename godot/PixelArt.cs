using Godot;

namespace Amphiteater.Godot;

/// <summary>
/// PixelLab paths from <c>assets/art/ASSET_MAP.md</c>. Missing files are skipped.
/// </summary>
static class PixelArt
{
    public const int Tile = 32;

    // STYLE.md packed_dirt swatch — yard fill when sample_dirt.png is absent.
    public static readonly Color PackedDirt = new(0x68 / 255f, 0x43 / 255f, 0x35 / 255f);

    public static string ArtRoot()
    {
        string project = ProjectSettings.GlobalizePath("res://");
        return Path.GetFullPath(Path.Combine(project, "..", "assets", "art"));
    }

    public static Texture2D? TryLoad(string relativeToArt)
    {
        string res = $"res://art/{relativeToArt}";
        if (ResourceLoader.Exists(res))
            return GD.Load<Texture2D>(res);

        string fs = Path.Combine(ArtRoot(), relativeToArt);
        if (!File.Exists(fs))
            return null;

        var img = Image.LoadFromFile(fs);
        if (img == null)
            return null;
        return ImageTexture.CreateFromImage(img);
    }

    public static Texture2D SolidTile(Color color)
    {
        var img = Image.CreateEmpty(Tile, Tile, false, Image.Format.Rgba8);
        img.Fill(color);
        return ImageTexture.CreateFromImage(img);
    }

    public static Sprite2D SpriteAt(Texture2D tex, Vector2I tile, int z)
    {
        var s = new Sprite2D
        {
            Texture = tex,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            Centered = false,
            Position = new Vector2(tile.X * Tile, tile.Y * Tile),
            ZIndex = z
        };
        return s;
    }
}
