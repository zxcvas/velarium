using Velarium;

namespace Amphiteater.Sim.Tests;

public class EquipmentAssignTests
{
    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    static EquipmentItem Buy(GameState s, EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier)
    {
        Assert.Null(Ludus.BuyEquipment(s, slot, culture, tier));
        return s.Armory[^1];
    }

    static void AssertEmptyLoadout(Gladiator g)
    {
        Assert.Null(g.Helmet);
        Assert.Null(g.Armor);
        Assert.Null(g.Shield);
        Assert.Null(g.Weapon);
    }

    [Fact]
    public void Assign_moves_item_from_armory_onto_living_man()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);

        Assert.Null(Ludus.Assign(s, murmillo, helm.Id));

        Assert.Empty(s.Armory);
        Assert.Same(helm, murmillo.Helmet);
        Assert.Equal(EquipmentSlot.Helmet, murmillo.Helmet!.Slot);
        Assert.Equal(EquipmentCulture.Roman, murmillo.Helmet.Culture);
        Assert.Equal(EquipmentTier.T1, murmillo.Helmet.Tier);
        Assert.Null(murmillo.Armor);
        Assert.Null(murmillo.Shield);
        Assert.Null(murmillo.Weapon);
    }

    [Fact]
    public void Assign_swap_returns_previous_piece_to_armory()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        var first = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        var second = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T2);
        Assert.Null(Ludus.Assign(s, murmillo, first.Id));
        Assert.Same(second, Assert.Single(s.Armory));
        Assert.Same(first, murmillo.Helmet);

        Assert.Null(Ludus.Assign(s, murmillo, second.Id));

        Assert.Same(second, murmillo.Helmet);
        var returned = Assert.Single(s.Armory);
        Assert.Same(first, returned);
        Assert.Equal(first.Id, returned.Id);
        Assert.Null(murmillo.Armor);
    }

    [Fact]
    public void Assign_rejects_retiarius_shield()
    {
        var s = Fresh();
        var retiarius = s.Living.Single(g => g.Armatura == Armatura.Retiarius);
        var shield = Buy(s, EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T1);
        Assert.False(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Shield));

        Assert.Equal("slot", Ludus.Assign(s, retiarius, shield.Id));

        Assert.Single(s.Armory);
        Assert.Same(shield, s.Armory[0]);
        Assert.Null(retiarius.Shield);
        AssertEmptyLoadout(retiarius);
    }

    [Fact]
    public void Assign_allows_retiarius_helmet_armor_and_any_culture_weapon()
    {
        var s = Fresh();
        var retiarius = s.Living.Single(g => g.Armatura == Armatura.Retiarius);
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T1);
        var armour = Buy(s, EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T2);
        var greekWeapon = Buy(s, EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T1);

        Assert.True(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Helmet));
        Assert.True(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Armor));
        Assert.True(EquipmentItem.SlotUsable(retiarius.Armatura, EquipmentSlot.Weapon));
        Assert.Null(Ludus.Assign(s, retiarius, helm.Id));
        Assert.Null(Ludus.Assign(s, retiarius, armour.Id));
        Assert.Null(Ludus.Assign(s, retiarius, greekWeapon.Id));

        Assert.Empty(s.Armory);
        Assert.Same(helm, retiarius.Helmet);
        Assert.Same(armour, retiarius.Armor);
        Assert.Null(retiarius.Shield);
        Assert.Same(greekWeapon, retiarius.Weapon);
        Assert.Equal(EquipmentCulture.Greek, retiarius.Weapon!.Culture);
    }

    [Fact]
    public void Unequip_returns_piece_to_armory()
    {
        var s = Fresh();
        var thraex = s.Living.Single(g => g.Armatura == Armatura.Thraex);
        var weapon = Buy(s, EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T3);
        Assert.Null(Ludus.Assign(s, thraex, weapon.Id));
        Assert.Empty(s.Armory);

        Assert.Null(Ludus.Unequip(s, thraex, EquipmentSlot.Weapon));

        Assert.Null(thraex.Weapon);
        var returned = Assert.Single(s.Armory);
        Assert.Same(weapon, returned);
        Assert.Equal("gone", Ludus.Unequip(s, thraex, EquipmentSlot.Weapon));
        Assert.Single(s.Armory);
    }

    [Fact]
    public void Kill_returns_loadout_to_armory_before_ad_libitinam()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2);
        var armour = Buy(s, EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T1);
        var shield = Buy(s, EquipmentSlot.Shield, EquipmentCulture.Greek, EquipmentTier.T1);
        var weapon = Buy(s, EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T3);
        Assert.Null(Ludus.Assign(s, murmillo, helm.Id));
        Assert.Null(Ludus.Assign(s, murmillo, armour.Id));
        Assert.Null(Ludus.Assign(s, murmillo, shield.Id));
        Assert.Null(Ludus.Assign(s, murmillo, weapon.Id));
        Assert.Empty(s.Armory);

        Ludus.Kill(s, murmillo);

        Assert.Equal(GladiatorStatus.Mortuus, murmillo.Status);
        Assert.False(murmillo.Alive);
        AssertEmptyLoadout(murmillo);
        Assert.Equal(4, s.Armory.Count);
        Assert.Contains(helm, s.Armory);
        Assert.Contains(armour, s.Armory);
        Assert.Contains(shield, s.Armory);
        Assert.Contains(weapon, s.Armory);
        Assert.Equal(4, s.Armory.Select(i => i.Id).Distinct().Count());
        Assert.Contains(s.AdLibitinam, line => line.Contains(murmillo.Name));
        Assert.Contains(s.Familia, x => x.Id == murmillo.Id);
        Assert.Equal("gone", Ludus.Assign(s, murmillo, helm.Id));
    }

    [Fact]
    public void GrantRudis_returns_loadout_to_armory()
    {
        var s = Fresh();
        s.Fama = Ludus.RudisFamaNeed;
        s.Denarii = 5_000;
        var g = s.Living.First();
        g.Palmae = Ludus.RudisPalmaeNeed;
        g.Virtus = 10;
        g.Fama = 4;
        g.VigorMax = 16;
        g.Vigor = 16;
        g.Status = GladiatorStatus.Validus;
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T1);
        var weapon = Buy(s, EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T1);
        Assert.Null(Ludus.Assign(s, g, helm.Id));
        Assert.Null(Ludus.Assign(s, g, weapon.Id));
        Assert.Empty(s.Armory);

        Assert.Null(Ludus.GrantRudis(s, g));

        Assert.DoesNotContain(s.Familia, x => x.Id == g.Id);
        AssertEmptyLoadout(g);
        Assert.Equal(2, s.Armory.Count);
        Assert.Contains(helm, s.Armory);
        Assert.Contains(weapon, s.Armory);
        Assert.Contains(s.Rudiarii, line => line.Contains(g.Name) && line.Contains("rudiarius"));
        Assert.DoesNotContain(s.AdLibitinam, line => line.Contains(g.Name));
    }

    [Fact]
    public void Assign_rejects_dead_stranger_or_missing_item()
    {
        var s = Fresh();
        var living = s.Living.First();
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        int purse = s.Denarii;

        var stranger = new Gladiator
        {
            Name = "NEMO",
            Status = GladiatorStatus.Validus,
            Vigor = 10,
            VigorMax = 10,
            Armatura = Armatura.Murmillo
        };
        Assert.Equal("gone", Ludus.Assign(s, stranger, helm.Id));
        Assert.Single(s.Armory);

        var corpse = s.Living.Skip(1).First();
        Ludus.Kill(s, corpse);
        Assert.Equal("gone", Ludus.Assign(s, corpse, helm.Id));
        Assert.Single(s.Armory);

        Assert.Equal("gone", Ludus.Assign(s, living, helm.Id + 99));
        Assert.Single(s.Armory);
        Assert.Null(Ludus.Assign(s, living, helm.Id));
        Assert.Equal("gone", Ludus.Assign(s, living, helm.Id));
        Assert.Same(helm, living.Helmet);
        Assert.Empty(s.Armory);
        Assert.Equal(purse, s.Denarii);
    }

    [Fact]
    public void GrantRudis_refusal_does_not_strip_kit()
    {
        var s = Fresh();
        s.Fama = 50;
        s.Denarii = 5_000;
        var g = s.Living.First();
        Assert.Equal(0, g.Palmae);
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        Assert.Null(Ludus.Assign(s, g, helm.Id));

        Assert.Equal("palmae", Ludus.GrantRudis(s, g));

        Assert.Contains(s.Living, x => x.Id == g.Id);
        Assert.Same(helm, g.Helmet);
        Assert.Empty(s.Armory);
        Assert.Empty(s.Rudiarii);
    }

    [Fact]
    public void SellEquipment_still_only_sells_from_armory()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        var helm = Buy(s, EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1);
        Assert.Null(Ludus.Assign(s, murmillo, helm.Id));
        int purse = s.Denarii;

        Assert.Equal("gone", Ludus.SellEquipment(s, 0));
        Assert.Equal(purse, s.Denarii);
        Assert.Same(helm, murmillo.Helmet);

        Assert.Null(Ludus.Unequip(s, murmillo, EquipmentSlot.Helmet));
        Assert.Null(Ludus.SellEquipment(s, 0));
        Assert.Empty(s.Armory);
        Assert.Null(murmillo.Helmet);
        Assert.Equal(purse + EquipmentCatalog.ResalePrice(helm), s.Denarii);
    }
}
