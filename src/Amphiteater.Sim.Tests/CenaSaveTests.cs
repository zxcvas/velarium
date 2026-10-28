using System.Text.Json.Nodes;
using Velarium;

namespace Amphiteater.Sim.Tests;

public class CenaSaveTests
{
    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    static GameState RoundTrip(GameState s)
    {
        string path = Path.Combine(Path.GetTempPath(), $"amphiteater_cena_{Guid.NewGuid():N}.json");
        try
        {
            Save.Write(path, s);
            var loaded = Save.Read(path);
            Assert.NotNull(loaded);
            return loaded!;
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    static void AssertCena(GameState s, bool pending, bool poison, int cooldown)
    {
        Assert.Equal(pending, s.CenaPending);
        Assert.Equal(poison, s.CenaPoison);
        Assert.Equal(cooldown, s.CenaCooldown);
    }

    [Fact]
    public void Start_has_no_pending_cena()
    {
        var s = Fresh();
        AssertCena(s, pending: false, poison: false, cooldown: 0);
    }

    [Fact]
    public void Save_round_trip_empty_cena()
    {
        var s = Fresh();
        var loaded = RoundTrip(s);
        AssertCena(loaded, pending: false, poison: false, cooldown: 0);
        Assert.Equal(s.Denarii, loaded.Denarii);
        Assert.Equal(s.Familia.Count, loaded.Familia.Count);
        Assert.NotNull(loaded.Rival);
        Assert.Equal(s.Rival!.Name, loaded.Rival!.Name);
    }

    [Fact]
    public void Save_round_trip_pending_hospitality()
    {
        var s = Fresh();
        s.CenaPending = true;
        s.CenaPoison = false;
        s.CenaCooldown = 0;

        var loaded = RoundTrip(s);
        AssertCena(loaded, pending: true, poison: false, cooldown: 0);
        Assert.Equal(s.NightOrder, loaded.NightOrder);
        Assert.Equal(s.Denarii, loaded.Denarii);
    }

    [Fact]
    public void Save_round_trip_pending_cup_and_cooldown()
    {
        var s = Fresh();
        s.CenaPending = true;
        s.CenaPoison = true;
        s.CenaCooldown = 4;

        var loaded = RoundTrip(s);
        AssertCena(loaded, pending: true, poison: true, cooldown: 4);
        Assert.False(loaded.Rival!.NextFoePoisoned);
        Assert.Equal(s.Fama, loaded.Fama);
        Assert.Equal(s.NextId, loaded.NextId);
    }

    [Fact]
    public void Save_writes_camel_case_cena_keys()
    {
        var s = Fresh();
        s.CenaPending = true;
        s.CenaPoison = true;
        s.CenaCooldown = 3;
        string path = Path.Combine(Path.GetTempPath(), $"amphiteater_cena_keys_{Guid.NewGuid():N}.json");
        try
        {
            Save.Write(path, s);
            var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            Assert.True(node["cenaPending"]!.GetValue<bool>());
            Assert.True(node["cenaPoison"]!.GetValue<bool>());
            Assert.Equal(3, node["cenaCooldown"]!.GetValue<int>());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_read_missing_cena_fields_defaults_false_false_zero()
    {
        var s = Fresh();
        s.CenaPending = true;
        s.CenaPoison = true;
        s.CenaCooldown = 4;
        string path = Path.Combine(Path.GetTempPath(), $"amphiteater_cena_old_{Guid.NewGuid():N}.json");
        try
        {
            Save.Write(path, s);
            var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            Assert.True(node.Remove("cenaPending"));
            Assert.True(node.Remove("cenaPoison"));
            Assert.True(node.Remove("cenaCooldown"));
            File.WriteAllText(path, node.ToJsonString());

            var loaded = Save.Read(path);
            Assert.NotNull(loaded);
            AssertCena(loaded!, pending: false, poison: false, cooldown: 0);
            Assert.Equal(s.Denarii, loaded!.Denarii);
            Assert.NotNull(loaded.Rival);
            Assert.Equal(s.Rival!.Name, loaded.Rival!.Name);
            Assert.False(loaded.CenaPending);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void EnsureHouse_does_not_invent_a_feast()
    {
        var s = Fresh();
        AssertCena(s, pending: false, poison: false, cooldown: 0);
        Ludus.EnsureHouse(s, new Random(1));
        AssertCena(s, pending: false, poison: false, cooldown: 0);

        s.CenaPending = true;
        s.CenaPoison = true;
        s.CenaCooldown = 2;
        Ludus.EnsureHouse(s, new Random(1));
        AssertCena(s, pending: true, poison: true, cooldown: 2);
    }
}
