using System.Text.Json.Nodes;
using Velarium;

namespace Amphiteater.Sim.Tests;

public class EquipmentSaveTests
{
    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    static GameState RoundTrip(GameState s)
    {
        string path = Path.Combine(Path.GetTempPath(), $"amphiteater_equip_{Guid.NewGuid():N}.json");
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

    static void AssertEmptyLoadout(Gladiator g)
    {
        Assert.Null(g.Helmet);
        Assert.Null(g.Armor);
        Assert.Null(g.Shield);
        Assert.Null(g.Weapon);
    }

    static EquipmentItem Mint(GameState s, EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
        => new()
        {
            Id = s.NextId++,
            Slot = slot,
            Culture = culture,
            Tier = tier
        };

    [Fact]
    public void Start_tiros_and_market_have_empty_armory_and_loadout()
    {
        var s = Fresh();
        Assert.Empty(s.Armory);
        Assert.All(s.Familia, AssertEmptyLoadout);
        Assert.All(s.Market, AssertEmptyLoadout);
        var retiarius = s.Living.Single(g => g.Armatura == Armatura.Retiarius);
        Assert.False(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Shield));
        Assert.True(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Weapon));
        Assert.True(EquipmentItem.SlotUsable(Armatura.Murmillo, EquipmentSlot.Shield));
    }

    [Fact]
    public void Save_round_trip_empty_armory()
    {
        var s = Fresh();
        Assert.Empty(s.Armory);
        var loaded = RoundTrip(s);
        Assert.NotNull(loaded.Armory);
        Assert.Empty(loaded.Armory);
        Assert.All(loaded.Familia, AssertEmptyLoadout);
        Assert.All(loaded.Market, AssertEmptyLoadout);
        Assert.Equal(s.Denarii, loaded.Denarii);
        Assert.Equal(s.Familia.Count, loaded.Familia.Count);
    }

    [Fact]
    public void Save_round_trip_buy_into_armory()
    {
        var s = Fresh();
        var helm = Mint(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        var armour = Mint(s, EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T2);
        var sameTemplate = Mint(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        s.Armory.Add(helm);
        s.Armory.Add(armour);
        s.Armory.Add(sameTemplate);
        Assert.NotEqual(helm.Id, sameTemplate.Id);

        var loaded = RoundTrip(s);
        Assert.Equal(3, loaded.Armory.Count);
        Assert.Equal(helm.Id, loaded.Armory[0].Id);
        Assert.Equal(EquipmentSlot.Helmet, loaded.Armory[0].Slot);
        Assert.Equal(EquipmentCulture.Roman, loaded.Armory[0].Culture);
        Assert.Equal(EquipmentTier.T1, loaded.Armory[0].Tier);
        Assert.Equal(armour.Id, loaded.Armory[1].Id);
        Assert.Equal(EquipmentSlot.Armor, loaded.Armory[1].Slot);
        Assert.Equal(EquipmentCulture.Greek, loaded.Armory[1].Culture);
        Assert.Equal(EquipmentTier.T2, loaded.Armory[1].Tier);
        Assert.Equal(sameTemplate.Id, loaded.Armory[2].Id);
        Assert.Equal(EquipmentSlot.Helmet, loaded.Armory[2].Slot);
        Assert.All(loaded.Familia, AssertEmptyLoadout);
        Assert.Equal(s.NextId, loaded.NextId);
    }

    [Fact]
    public void Save_round_trip_partial_loadout()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        var retiarius = s.Living.Single(g => g.Armatura == Armatura.Retiarius);
        murmillo.Helmet = Mint(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2);
        murmillo.Weapon = Mint(s, EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T3);
        retiarius.Armor = Mint(s, EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T1);
        Assert.Null(murmillo.Armor);
        Assert.Null(murmillo.Shield);
        Assert.Null(retiarius.Helmet);
        Assert.Null(retiarius.Shield);
        Assert.Null(retiarius.Weapon);
        Assert.False(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Shield));

        var loaded = RoundTrip(s);
        var loadedMurmillo = loaded.Living.Single(g => g.Armatura == Armatura.Murmillo);
        var loadedRetiarius = loaded.Living.Single(g => g.Armatura == Armatura.Retiarius);
        var loadedThraex = loaded.Living.Single(g => g.Armatura == Armatura.Thraex);

        Assert.NotNull(loadedMurmillo.Helmet);
        Assert.Equal(murmillo.Helmet.Id, loadedMurmillo.Helmet!.Id);
        Assert.Equal(EquipmentSlot.Helmet, loadedMurmillo.Helmet.Slot);
        Assert.Equal(EquipmentCulture.Roman, loadedMurmillo.Helmet.Culture);
        Assert.Equal(EquipmentTier.T2, loadedMurmillo.Helmet.Tier);
        Assert.Null(loadedMurmillo.Armor);
        Assert.Null(loadedMurmillo.Shield);
        Assert.NotNull(loadedMurmillo.Weapon);
        Assert.Equal(murmillo.Weapon.Id, loadedMurmillo.Weapon!.Id);
        Assert.Equal(EquipmentCulture.Punic, loadedMurmillo.Weapon.Culture);
        Assert.Equal(EquipmentTier.T3, loadedMurmillo.Weapon.Tier);

        Assert.Null(loadedRetiarius.Helmet);
        Assert.NotNull(loadedRetiarius.Armor);
        Assert.Equal(retiarius.Armor.Id, loadedRetiarius.Armor!.Id);
        Assert.Null(loadedRetiarius.Shield);
        Assert.Null(loadedRetiarius.Weapon);
        AssertEmptyLoadout(loadedThraex);
        Assert.Empty(loaded.Armory);
    }

    [Fact]
    public void Save_read_missing_armory_and_loadout_defaults_empty()
    {
        var s = Fresh();
        string path = Path.Combine(Path.GetTempPath(), $"amphiteater_equip_old_{Guid.NewGuid():N}.json");
        try
        {
            Save.Write(path, s);
            var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            node.Remove("armory");
            foreach (var listName in new[] { "familia", "market" })
            {
                foreach (var g in node[listName]!.AsArray())
                {
                    var o = g!.AsObject();
                    o.Remove("helmet");
                    o.Remove("armor");
                    o.Remove("shield");
                    o.Remove("weapon");
                }
            }
            File.WriteAllText(path, node.ToJsonString());
            var loaded = Save.Read(path);
            Assert.NotNull(loaded);
            Assert.NotNull(loaded!.Armory);
            Assert.Empty(loaded.Armory);
            Assert.All(loaded.Familia, AssertEmptyLoadout);
            Assert.All(loaded.Market, AssertEmptyLoadout);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
