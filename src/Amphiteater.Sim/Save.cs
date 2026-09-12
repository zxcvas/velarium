using System.Text.Json;

namespace Velarium;

public static class Save
{
    public const string FileName = "amphiteater_save.json";

    static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = false
    };

    public static string DefaultPath()
        => Path.Combine(AppContext.BaseDirectory, FileName);

    public static bool Exists(string path) => File.Exists(path);

    public static void Write(string path, GameState state)
    {
        string json = JsonSerializer.Serialize(state, Opts);
        File.WriteAllText(path, json);
    }

    public static GameState? Read(string path)
    {
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        var s = JsonSerializer.Deserialize<GameState>(json, Opts);
        if (s != null)
            Ludus.EnsureHouse(s, new Random(unchecked(s.Seed + s.DaysPlayed * 17)));
        return s;
    }
}
