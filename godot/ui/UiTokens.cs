using Godot;
using Velarium;

namespace Amphiteater.Godot;

/// <summary>
/// Velarium UI tokens. Primitives are the STYLE.md palette; roles mirror
/// <c>marketing/site/styles.css</c>. Table: <c>docs/design/03_ui_tokens.md</c>.
/// Sizes are viewport px: the 320×180 viewport is drawn at integer 4×, so
/// one unit here is one art pixel (= 4px on the site).
/// </summary>
static class UiTokens
{
    // Primitives — assets/art/STYLE.md, do not add colours here.
    public static readonly Color Soot = Color.FromHtml("#221F22");
    public static readonly Color CharcoalDeep = Color.FromHtml("#372E31");
    public static readonly Color Charcoal = Color.FromHtml("#444548");
    public static readonly Color Iron = Color.FromHtml("#505455");
    public static readonly Color PackedDirt = Color.FromHtml("#684335");
    public static readonly Color IronRust = Color.FromHtml("#876C57");
    public static readonly Color DirtyLinen = Color.FromHtml("#B7A181");
    public static readonly Color PaleLinen = Color.FromHtml("#E8DCC0");
    public static readonly Color Ochre = Color.FromHtml("#BB8715");
    public static readonly Color PompeiiRed = Color.FromHtml("#A44A3E");
    public static readonly Color DarkPompeii = Color.FromHtml("#5B2D2D");
    public static readonly Color SeaGreen = Color.FromHtml("#4A8B7A");
    public static readonly Color ColdIron = Color.FromHtml("#7F99B0");
    public static readonly Color Bronze = Color.FromHtml("#C4964A");
    public static readonly Color Skin = Color.FromHtml("#C8A87A");
    public static readonly Color Outline = Color.FromHtml("#0A0808");

    // Semantic roles — UI code asks for these, not primitives.
    public static Color SurfaceGround => Soot;
    public static Color SurfaceRaised => CharcoalDeep;
    public static Color SurfaceBanner => DarkPompeii;
    public static Color SurfaceSand => PackedDirt;
    public static Color BorderChrome => Outline;
    public static Color TextPrimary => PaleLinen;
    /// <summary>Not IronRust: that is 3.4:1 on ground and fails as text.</summary>
    public static Color TextSecondary => DirtyLinen;
    public static Color TextLink => Bronze;
    public static Color FocusRing => Ochre;
    public static Color ActionPrimary => PompeiiRed;
    public static Color ActionPrimaryHover => DarkPompeii;
    public static Color ActionSecondary => CharcoalDeep;
    public static Color ActionDisabled => Charcoal;
    public static Color Selected => Bronze;

    // Spacing: Space(n) here = --space-n on the site (n × 4 screen px).
    public const int Space1 = 1;
    public const int Space2 = 2;
    public const int Space3 = 3;
    public const int Space4 = 4;
    public const int Space6 = 6;
    public const int Space8 = 8;
    public const int Space12 = 12;
    public const int Space16 = 16;

    /// <summary>Chrome border. The site's 3px rounds to one art pixel.</summary>
    public const int Border = 1;
    /// <summary>Never rounded.</summary>
    public const int Radius = 0;

    // Silkscreen (PixelFont) sizes in viewport px — whole multiples of 8 only,
    // so glyphs stay on the art-pixel grid. ×4 on screen: all HUD text is "large".
    public const int FontBody = 8;
    public const int FontTitle = 16;

    public readonly record struct StatusStyle(Color Fill, Color Text, Color Rim);

    /// <summary>
    /// Tag colours for <see cref="GladiatorStatus"/>. Light fills take outline
    /// text; red cannot, so Vulneratus inverts to a dark fill with a red rim.
    /// </summary>
    public static StatusStyle Status(GladiatorStatus status) => status switch
    {
        GladiatorStatus.Validus => new(SeaGreen, Outline, Outline),
        GladiatorStatus.Fessus => new(Ochre, Outline, Outline),
        GladiatorStatus.Vulneratus => new(DarkPompeii, PaleLinen, PompeiiRed),
        GladiatorStatus.Aeger => new(ColdIron, Outline, Outline),
        GladiatorStatus.Mortuus => new(Soot, DirtyLinen, Charcoal),
        _ => new(Charcoal, PaleLinen, Outline)
    };
}
