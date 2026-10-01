using Godot;

namespace Amphiteater.Godot;

/// <summary>
/// Builds the HUD <see cref="Theme"/> from <see cref="UiTokens"/>: square
/// chrome, one-pixel outline, Pompeii-red primary. Code-built so the editor
/// is not needed to change it.
/// </summary>
static class VelariumTheme
{
    /// <summary>Type variation for secondary buttons: <c>button.ThemeTypeVariation = "SecondaryButton"</c>.</summary>
    public const string SecondaryButton = "SecondaryButton";

    public static Theme Build()
    {
        var theme = new Theme { DefaultFontSize = UiTokens.FontBody };

        ButtonStyles(theme, "Button", UiTokens.ActionPrimary, UiTokens.ActionPrimaryHover);
        theme.SetTypeVariation(SecondaryButton, "Button");
        ButtonStyles(theme, SecondaryButton, UiTokens.ActionSecondary, UiTokens.ActionSecondary);
        theme.SetColor("font_hover_color", SecondaryButton, UiTokens.FocusRing);

        theme.SetColor("font_color", "Label", UiTokens.TextPrimary);

        var panel = Box(UiTokens.SurfaceRaised, UiTokens.BorderChrome);
        panel.SetContentMarginAll(UiTokens.Space4);
        theme.SetStylebox("panel", "PanelContainer", panel);
        theme.SetStylebox("panel", "Panel", panel);

        return theme;
    }

    static void ButtonStyles(Theme theme, string type, Color fill, Color hover)
    {
        theme.SetStylebox("normal", type, Box(fill, UiTokens.BorderChrome));
        theme.SetStylebox("hover", type, Box(hover, UiTokens.BorderChrome));
        theme.SetStylebox("pressed", type, Box(UiTokens.Selected, UiTokens.BorderChrome));
        theme.SetStylebox("disabled", type, Box(UiTokens.ActionDisabled, UiTokens.BorderChrome));

        var focus = Box(Colors.Transparent, UiTokens.FocusRing);
        focus.DrawCenter = false;
        focus.SetExpandMarginAll(UiTokens.Space1);
        theme.SetStylebox("focus", type, focus);

        theme.SetColor("font_color", type, UiTokens.TextPrimary);
        theme.SetColor("font_hover_color", type, UiTokens.TextPrimary);
        theme.SetColor("font_focus_color", type, UiTokens.TextPrimary);
        theme.SetColor("font_pressed_color", type, UiTokens.Outline);
        theme.SetColor("font_disabled_color", type, UiTokens.TextSecondary);
    }

    static StyleBoxFlat Box(Color fill, Color border)
    {
        var box = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            AntiAliasing = false
        };
        box.SetBorderWidthAll(UiTokens.Border);
        box.SetCornerRadiusAll(UiTokens.Radius);
        box.SetContentMarginAll(UiTokens.Space2);
        return box;
    }
}
