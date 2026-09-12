using Godot;
using Velarium;

namespace Amphiteater.Godot;

/// <summary>
/// Presentation only: dirt yard, wave-1 props, murmillo south idle, same JSON save.
/// No new sim verbs — End Day is <see cref="Ludus.EndDay"/>.
/// </summary>
public partial class Courtyard : Node2D
{
    const int TilesX = 10;
    const int TilesY = 5;

    static readonly (string File, Vector2I Tile, RoomKind? Room)[] Wave1 =
    {
        ("props/prop_palus.png", new Vector2I(4, 2), RoomKind.Palus),
        ("props/prop_cellae.png", new Vector2I(1, 1), RoomKind.Cellae),
        ("props/prop_cellae.png", new Vector2I(8, 1), RoomKind.Cellae),
        ("props/prop_hearth.png", new Vector2I(8, 3), RoomKind.Kitchen),
        ("props/prop_porta.png", new Vector2I(4, 4), RoomKind.Porta),
        ("props/prop_medicus.png", new Vector2I(1, 3), RoomKind.Medicus)
    };

    GameState _state = null!;
    string _savePath = "";
    Label _status = null!;

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        _status = GetNode<Label>("Hud/Status");
        GetNode<Button>("Hud/EndDay").Pressed += OnEndDay;

        _savePath = ResolveSavePath();
        _state = Save.Read(_savePath) ?? Ludus.Start(new Random(782), 782, "Lucius", "Atinius", "Strabo");

        FillDirt();
        PlaceWave1Props();
        PlaceMurmilloSouth();
        RefreshHud("courtyard");
    }

    void FillDirt()
    {
        var yard = GetNode<Node2D>("Yard");
        var tex = PixelArt.TryLoad("tiles/sample_dirt.png") ?? PixelArt.SolidTile(PixelArt.PackedDirt);
        for (int y = 0; y < TilesY; y++)
        {
            for (int x = 0; x < TilesX; x++)
                yard.AddChild(PixelArt.SpriteAt(tex, new Vector2I(x, y), 0));
        }
    }

    void PlaceWave1Props()
    {
        var props = GetNode<Node2D>("Props");
        foreach (var (file, tile, room) in Wave1)
        {
            if (room is RoomKind kind)
            {
                var built = Ludus.RoomOf(_state, kind);
                if (built is { Built: false })
                    continue;
            }

            var tex = PixelArt.TryLoad(file);
            if (tex == null)
                continue;
            props.AddChild(PixelArt.SpriteAt(tex, tile, 1));
        }
    }

    void PlaceMurmilloSouth()
    {
        var tex = PixelArt.TryLoad("characters/murmillo_s_idle_00.png")
            ?? PixelArt.TryLoad("characters/sample_murmillo_s.png");
        if (tex == null)
            return;
        if (!_state.Living.Any(g => g.Armatura == Armatura.Murmillo))
            return;
        GetNode<Node2D>("Familia").AddChild(PixelArt.SpriteAt(tex, new Vector2I(5, 2), 2));
    }

    void OnEndDay()
    {
        var rng = new Random(unchecked(_state.Seed + _state.DaysPlayed * 17));
        var night = Ludus.EndDay(_state, rng);
        Save.Write(_savePath, _state);
        string last = night.Log.Count > 0 ? night.Log[^1] : "night";
        RefreshHud(last);
    }

    void RefreshHud(string note)
    {
        _status.Text = $"{Calendar.Format(_state)}  {_state.Denarii} denarii  fama {_state.Fama}\n{note}";
    }

    static string ResolveSavePath()
    {
        string project = ProjectSettings.GlobalizePath("res://");
        string dist = Path.GetFullPath(Path.Combine(project, "..", "dist", Save.FileName));
        if (File.Exists(dist))
            return dist;

        string user = Path.Combine(OS.GetUserDataDir(), Save.FileName);
        if (File.Exists(user))
            return user;

        return File.Exists(Save.DefaultPath()) ? Save.DefaultPath() : user;
    }
}
