using System.Text.Json;

namespace Velarium;

static class Save
{
    static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = false
    };

    public static string DefaultPath()
        => Path.Combine(AppContext.BaseDirectory, "amphiteater_save.json");

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
        return JsonSerializer.Deserialize<GameState>(json, Opts);
    }
}
