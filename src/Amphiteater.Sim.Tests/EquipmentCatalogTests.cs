using Velarium;

namespace Amphiteater.Sim.Tests;

public class EquipmentCatalogTests
{
    // Sheet: production/economy.md § Forum equipment → Catalog buy prices (denarii).
    // Rows = Helmet, Armor, Shield, Weapon; cols = Punic, Greek, Roman; each cell T1/T2/T3.
    static readonly int[,,] EconomyBuy =
    {
        // Helmet
        { { 35, 55, 80 }, { 40, 60, 90 }, { 45, 70, 100 } },
        // Armor
        { { 50, 80, 120 }, { 55, 90, 130 }, { 60, 100, 150 } },
        // Shield
        { { 25, 40, 60 }, { 30, 45, 70 }, { 35, 55, 85 } },
        // Weapon
        { { 30, 50, 75 }, { 35, 55, 85 }, { 40, 60, 95 } },
    };

    static IEnumerable<(EquipmentSlot Slot, EquipmentCulture Culture, EquipmentTier Tier)> AllCells()
    {
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        foreach (var culture in Enum.GetValues<EquipmentCulture>())
        foreach (var tier in Enum.GetValues<EquipmentTier>())
            yield return (slot, culture, tier);
    }

    static int SheetBuy(EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
        => EconomyBuy[(int)slot, (int)culture, (int)tier];

    [Fact]
    public void Catalog_has_every_culture_slot_tier_cell()
    {
        Assert.Equal(0, (int)EquipmentSlot.Helmet);
        Assert.Equal(1, (int)EquipmentSlot.Armor);
        Assert.Equal(2, (int)EquipmentSlot.Shield);
        Assert.Equal(3, (int)EquipmentSlot.Weapon);
        Assert.Equal(0, (int)EquipmentCulture.Punic);
        Assert.Equal(1, (int)EquipmentCulture.Greek);
        Assert.Equal(2, (int)EquipmentCulture.Roman);
        Assert.Equal(0, (int)EquipmentTier.T1);
        Assert.Equal(1, (int)EquipmentTier.T2);
        Assert.Equal(2, (int)EquipmentTier.T3);
        Assert.Equal(4, Enum.GetValues<EquipmentSlot>().Length);
        Assert.Equal(3, Enum.GetValues<EquipmentCulture>().Length);
        Assert.Equal(3, Enum.GetValues<EquipmentTier>().Length);
        Assert.Equal(36, AllCells().Count());
        Assert.Equal(36, EquipmentCatalog.Templates.Length);
        Assert.Equal(36, EquipmentCatalog.Templates.Distinct().Count());
        foreach (var (slot, culture, tier) in AllCells())
            Assert.Contains(EquipmentCatalog.Templates, t => t.Slot == slot && t.Culture == culture && t.Tier == tier);
    }

    [Fact]
    public void BuyPrice_matches_economy_md_for_every_cell()
    {
        foreach (var (slot, culture, tier) in AllCells())
            Assert.Equal(SheetBuy(slot, culture, tier), EquipmentCatalog.BuyPrice(slot, culture, tier));
    }

    [Fact]
    public void ResalePrice_is_floor_half_of_buy_for_every_cell()
    {
        foreach (var (slot, culture, tier) in AllCells())
        {
            int buy = SheetBuy(slot, culture, tier);
            int expected = buy / 2;
            Assert.Equal(expected, EquipmentCatalog.ResalePrice(slot, culture, tier));
            Assert.True(expected * 2 <= buy);
            Assert.True(expected * 2 + 1 >= buy);
        }

        Assert.Equal(17, EquipmentCatalog.ResalePrice(EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T1));
        Assert.Equal(50, EquipmentCatalog.ResalePrice(EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T2));
    }

    [Fact]
    public void CreateInstance_sets_slot_culture_tier_and_unique_id()
    {
        var a = EquipmentCatalog.CreateInstance(7, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2);
        var b = EquipmentCatalog.CreateInstance(8, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2);
        var c = EquipmentCatalog.CreateInstance(9, EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T3);

        Assert.Equal(7, a.Id);
        Assert.Equal(EquipmentSlot.Helmet, a.Slot);
        Assert.Equal(EquipmentCulture.Roman, a.Culture);
        Assert.Equal(EquipmentTier.T2, a.Tier);

        Assert.Equal(8, b.Id);
        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal(a.Slot, b.Slot);
        Assert.Equal(a.Culture, b.Culture);
        Assert.Equal(a.Tier, b.Tier);

        Assert.Equal(9, c.Id);
        Assert.Equal(EquipmentSlot.Weapon, c.Slot);
        Assert.Equal(EquipmentCulture.Punic, c.Culture);
        Assert.Equal(EquipmentTier.T3, c.Tier);

        Assert.Equal(EquipmentCatalog.BuyPrice(a.Slot, a.Culture, a.Tier), EquipmentCatalog.BuyPrice(a));
        Assert.Equal(EquipmentCatalog.ResalePrice(c.Slot, c.Culture, c.Tier), EquipmentCatalog.ResalePrice(c));
    }

    [Fact]
    public void EquipmentNom_every_cell_is_short_unique_and_named()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (slot, culture, tier) in AllCells())
        {
            string nom = Content.EquipmentNom(slot, culture, tier);
            Assert.False(string.IsNullOrWhiteSpace(nom));
            Assert.True(nom.Length <= 28, nom);
            Assert.Contains(Content.EquipmentCultureNom(culture), nom);
            Assert.Contains(Content.EquipmentTierNom(tier), nom);
            Assert.True(names.Add(nom), nom);
        }

        var item = EquipmentCatalog.CreateInstance(1, EquipmentSlot.Shield, EquipmentCulture.Greek, EquipmentTier.T1);
        Assert.Equal(Content.EquipmentNom(EquipmentSlot.Shield, EquipmentCulture.Greek, EquipmentTier.T1), Content.EquipmentNom(item));
    }

    [Fact]
    public void EquipmentNom_roman_weapon_is_net_and_trident()
    {
        foreach (var tier in Enum.GetValues<EquipmentTier>())
        {
            string nom = Content.EquipmentNom(EquipmentSlot.Weapon, EquipmentCulture.Roman, tier);
            Assert.Contains("rete", nom, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("fuscina", nom, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Unknown_catalog_cell_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EquipmentCatalog.BuyPrice((EquipmentSlot)99, EquipmentCulture.Roman, EquipmentTier.T1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EquipmentCatalog.CreateInstance(1, EquipmentSlot.Helmet, (EquipmentCulture)99, EquipmentTier.T1));
    }
}
