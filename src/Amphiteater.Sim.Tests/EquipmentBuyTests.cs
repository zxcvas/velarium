using Velarium;

namespace Amphiteater.Sim.Tests;

public class EquipmentBuyTests
{
    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    static HashSet<int> TakenIds(GameState s)
    {
        var ids = s.Familia.Select(g => g.Id)
            .Concat(s.Market.Select(g => g.Id))
            .Concat(s.Household.Select(w => w.Id))
            .Concat(s.LaborMarket.Select(w => w.Id))
            .Concat(s.Armory.Select(i => i.Id));
        return ids.ToHashSet();
    }

    [Fact]
    public void Start_armory_is_empty_and_tiros_have_no_kit()
    {
        var s = Fresh();
        Assert.Empty(s.Armory);
        Assert.All(s.Living, g =>
        {
            Assert.Null(g.Helmet);
            Assert.Null(g.Armor);
            Assert.Null(g.Shield);
            Assert.Null(g.Weapon);
        });
    }

    [Fact]
    public void BuyEquipment_debits_purse_and_adds_instance_to_armory()
    {
        var s = Fresh();
        int purse = s.Denarii;
        int next = s.NextId;
        var taken = TakenIds(s);
        int price = EquipmentCatalog.BuyPrice(EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        Assert.Equal(45, price);

        Assert.Null(Ludus.BuyEquipment(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1));

        Assert.Equal(purse - 45, s.Denarii);
        Assert.Single(s.Armory);
        var item = s.Armory[0];
        Assert.Equal(next, item.Id);
        Assert.Equal(EquipmentSlot.Helmet, item.Slot);
        Assert.Equal(EquipmentCulture.Roman, item.Culture);
        Assert.Equal(EquipmentTier.T1, item.Tier);
        Assert.False(taken.Contains(item.Id));
        Assert.Equal(next + 1, s.NextId);
        Assert.All(s.Living, g => Assert.Null(g.Helmet));
    }

    [Fact]
    public void BuyEquipment_template_overload_matches_slot_culture_tier()
    {
        var s = Fresh();
        var t = EquipmentCatalog.Template(EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T2);
        int purse = s.Denarii;
        Assert.Null(Ludus.BuyEquipment(s, t));
        Assert.Equal(purse - t.Buy, s.Denarii);
        Assert.Single(s.Armory);
        Assert.Equal(EquipmentSlot.Weapon, s.Armory[0].Slot);
        Assert.Equal(EquipmentCulture.Greek, s.Armory[0].Culture);
        Assert.Equal(EquipmentTier.T2, s.Armory[0].Tier);
    }

    [Fact]
    public void BuyEquipment_rejects_insufficient_funds()
    {
        var s = Fresh();
        int price = EquipmentCatalog.BuyPrice(EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T3);
        s.Denarii = price - 1;
        int purse = s.Denarii;
        int next = s.NextId;

        Assert.Equal("coin", Ludus.BuyEquipment(s, EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T3));

        Assert.Equal(purse, s.Denarii);
        Assert.Empty(s.Armory);
        Assert.Equal(next, s.NextId);
    }

    [Fact]
    public void BuyEquipment_exact_purse_leaves_zero()
    {
        var s = Fresh();
        s.Denarii = EquipmentCatalog.BuyPrice(EquipmentSlot.Shield, EquipmentCulture.Punic, EquipmentTier.T1);
        Assert.Null(Ludus.BuyEquipment(s, EquipmentSlot.Shield, EquipmentCulture.Punic, EquipmentTier.T1));
        Assert.Equal(0, s.Denarii);
        Assert.Single(s.Armory);
        Assert.Equal(EquipmentSlot.Shield, s.Armory[0].Slot);
        Assert.Equal(EquipmentCulture.Punic, s.Armory[0].Culture);
        Assert.Equal(EquipmentTier.T1, s.Armory[0].Tier);
    }

    [Fact]
    public void BuyEquipment_two_of_the_same_template_have_unique_ids()
    {
        var s = Fresh();
        var taken = TakenIds(s);
        Assert.Null(Ludus.BuyEquipment(s, EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T2));
        Assert.Null(Ludus.BuyEquipment(s, EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T2));

        Assert.Equal(2, s.Armory.Count);
        var a = s.Armory[0];
        var b = s.Armory[1];
        Assert.NotEqual(a.Id, b.Id);
        Assert.False(taken.Contains(a.Id));
        Assert.False(taken.Contains(b.Id));
        Assert.Equal(a.Slot, b.Slot);
        Assert.Equal(a.Culture, b.Culture);
        Assert.Equal(a.Tier, b.Tier);
        Assert.Equal(2, s.Armory.Select(i => i.Id).Distinct().Count());
        Assert.Equal(s.Armory.Select(i => i.Id).Concat(taken).Distinct().Count(),
            s.Armory.Count + taken.Count);
    }

    [Fact]
    public void SellEquipment_credits_resale_and_removes_from_armory()
    {
        var s = Fresh();
        int start = s.Denarii;
        Assert.Null(Ludus.BuyEquipment(s, EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T2));
        var item = Assert.Single(s.Armory);
        int resale = EquipmentCatalog.ResalePrice(item);
        Assert.Equal(50, resale);

        Assert.Null(Ludus.SellEquipment(s, 0));

        Assert.Empty(s.Armory);
        Assert.Equal(start - 100 + 50, s.Denarii);
    }

    [Fact]
    public void SellEquipment_rejects_empty_or_bad_index()
    {
        var s = Fresh();
        int purse = s.Denarii;
        Assert.Equal("gone", Ludus.SellEquipment(s, 0));
        Assert.Equal("gone", Ludus.SellEquipment(s, -1));
        Assert.Equal(purse, s.Denarii);

        Assert.Null(Ludus.BuyEquipment(s, EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T1));
        Assert.Equal("gone", Ludus.SellEquipment(s, 1));
        Assert.Single(s.Armory);
    }

    [Fact]
    public void CareerSim_does_not_auto_buy_kit()
    {
        for (int seed = 1; seed <= 6; seed++)
        {
            var stats = CareerSim.RunCareer(seed, maxDays: 12);
            Assert.Equal(0, stats.ArmoryCount);
        }

        var locatio = CareerSim.RunCareer(3, maxDays: 12, CareerKitchenPolicy.LocatioOnly);
        Assert.Equal(0, locatio.ArmoryCount);
    }
}
