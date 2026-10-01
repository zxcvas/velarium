using Godot;

namespace Amphiteater.Godot;

/// <summary>
/// Silkscreen (OFL, <c>fonts/OFL.txt</c>): all-caps pixel face, crisp at 8 and
/// 16 viewport px. Loaded with antialiasing and hinting off so glyphs land on
/// whole art pixels; null when the file is missing (theme keeps the default).
/// </summary>
static class PixelFont
{
    public const string Regular = "res://fonts/Silkscreen-Regular.ttf";
    public const string Bold = "res://fonts/Silkscreen-Bold.ttf";

    public static FontFile? TryLoad(string path)
    {
        FontFile? font = null;
        if (ResourceLoader.Exists(path))
            font = ResourceLoader.Load<FontFile>(path)?.Duplicate() as FontFile;

        if (font == null)
        {
            if (!global::Godot.FileAccess.FileExists(path))
                return null;
            font = new FontFile { Data = global::Godot.FileAccess.GetFileAsBytes(path) };
        }

        font.Antialiasing = TextServer.FontAntialiasing.None;
        font.Hinting = TextServer.Hinting.None;
        font.SubpixelPositioning = TextServer.SubpixelPositioning.Disabled;
        font.GenerateMipmaps = false;
        font.MultichannelSignedDistanceField = false;
        return font;
    }
}
