using Velarium;

namespace Amphiteater.Sim.Tests;

public class EquipmentCombatTests
{
    static Gladiator Man(Armatura a, int virtus = 6, int pugnat = 3) => new()
    {
        Name = a.ToString(),
        Armatura = a,
        Virtus = virtus,
        VigorMax = 16,
        Vigor = 16,
        Pugnat = pugnat,
        Status = GladiatorStatus.Validus,
        Source = "servus"
    };

    static EquipmentItem Piece(EquipmentSlot slot, EquipmentCulture culture, EquipmentTier tier, int id = 1)
        => new() { Id = id, Slot = slot, Culture = culture, Tier = tier };

    static Gladiator Kit(Gladiator g, params (EquipmentSlot Slot, EquipmentCulture Culture, EquipmentTier Tier)[] pieces)
    {
        int id = 1;
        foreach (var p in pieces)
            g.SetEquipped(p.Slot, Piece(p.Slot, p.Culture, p.Tier, id++));
        return g;
    }

    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    static void AssertScoreAppliesEquipment(Gladiator g)
    {
        var foe = Man(Content.ClassicFoe(g.Armatura));
        const int vigor = 16;
        int with = Combat.Score(new Random(11), g, foe, vigor);
        int without = Combat.Score(new Random(11), g, foe, vigor, applyEquipment: false);
        Assert.Equal(Combat.EquipmentScore(g), with - without);
    }

    [Fact]
    public void Tiro_virtus_vigor_bands_and_bare_score_formula_unchanged()
    {
        var s = Fresh();
        Assert.All(s.Living, g =>
        {
            Assert.InRange(g.Virtus, 4, 7);
            Assert.InRange(g.VigorMax, 14, 18);
            Assert.Equal(0, g.Pugnat);
        });

        var vetRng = new Random(2);
        var vet = Ludus.MakeGladiator(s, vetRng, new HashSet<string>(), Armatura.Secutor, tiro: false);
        Assert.InRange(vet.Virtus, 7, 11);

        var self = Man(Armatura.Murmillo, virtus: 6, pugnat: 3);
        var other = Man(Armatura.Thraex);
        const int vigor = 16;
        var dice = new Random(42);
        int expected = 6 + vigor / 4 + dice.Next(1, 7) + dice.Next(1, 7) + 1;
        Assert.Equal(expected, Combat.Score(new Random(42), self, other, vigor));
        Assert.Equal(0, Combat.EquipmentScore(self));
    }

    [Fact]
    public void Empty_slots_on_heavy_armaturae_have_no_naked_penalty()
    {
        Assert.Equal(0, Combat.EquipmentScore(Man(Armatura.Murmillo)));
        Assert.Equal(0, Combat.EquipmentScore(Man(Armatura.Thraex)));
        Assert.Equal(0, Combat.EquipmentScore(Man(Armatura.Secutor)));
        AssertScoreAppliesEquipment(Man(Armatura.Murmillo));
    }

    [Fact]
    public void T1_pieces_add_no_bonus()
    {
        var g = Kit(Man(Armatura.Murmillo),
            (EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1),
            (EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T1),
            (EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T1),
            (EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T1));
        Assert.Equal(Combat.GearTier1Bonus, Combat.TierBonus(EquipmentTier.T1));
        Assert.Equal(0, Combat.EquipmentScore(g));
        Assert.False(Combat.CultureMismatch(g));
        AssertScoreAppliesEquipment(g);

        var foe = Man(Armatura.Thraex);
        Assert.Equal(
            Combat.Score(new Random(5), Man(Armatura.Murmillo), foe, 16),
            Combat.Score(new Random(5), g, foe, 16));
    }

    [Fact]
    public void T3_pieces_cap_total_gear_bonus_at_four()
    {
        var g = Kit(Man(Armatura.Murmillo),
            (EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T3),
            (EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T3),
            (EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T3),
            (EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T3));
        Assert.Equal(Combat.GearTier3Bonus, Combat.TierBonus(EquipmentTier.T3));
        Assert.Equal(Combat.GearBonusCap, Combat.EquipmentScore(g));
        Assert.True(4 * Combat.GearTier3Bonus > Combat.GearBonusCap);
        AssertScoreAppliesEquipment(g);
    }

    [Fact]
    public void Three_T2_pieces_stay_under_the_cap()
    {
        var g = Kit(Man(Armatura.Murmillo),
            (EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2),
            (EquipmentSlot.Armor, EquipmentCulture.Roman, EquipmentTier.T2),
            (EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T2));
        Assert.Equal(3 * Combat.GearTier2Bonus, Combat.EquipmentScore(g));
        Assert.True(Combat.EquipmentScore(g) < Combat.GearBonusCap);
    }

    [Fact]
    public void Retiarius_without_roman_weapon_is_minus_two()
    {
        var bare = Man(Armatura.Retiarius);
        Assert.Equal(-Combat.RetiariusMissingRomanWeapon, Combat.EquipmentScore(bare));
        AssertScoreAppliesEquipment(bare);

        var greek = Kit(Man(Armatura.Retiarius),
            (EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T1));
        Assert.False(Combat.HasRomanWeapon(greek));
        Assert.False(Combat.CultureMismatch(greek));
        Assert.Equal(-Combat.RetiariusMissingRomanWeapon, Combat.EquipmentScore(greek));
    }

    [Fact]
    public void Retiarius_t1_roman_weapon_clears_the_minus_two()
    {
        var g = Kit(Man(Armatura.Retiarius),
            (EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T1));
        Assert.True(Combat.HasRomanWeapon(g));
        Assert.Equal(0, Combat.EquipmentScore(g));
        Assert.False(Combat.CultureMismatch(g));
        AssertScoreAppliesEquipment(g);
    }

    [Fact]
    public void Mismatch_is_minus_one_once_not_per_piece()
    {
        var one = Kit(Man(Armatura.Murmillo),
            (EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T1));
        var two = Kit(Man(Armatura.Murmillo),
            (EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T1),
            (EquipmentSlot.Armor, EquipmentCulture.Punic, EquipmentTier.T1));
        Assert.True(Combat.CultureMismatch(one));
        Assert.True(Combat.CultureMismatch(two));
        Assert.Equal(-Combat.CultureMismatchDock, Combat.EquipmentScore(one));
        Assert.Equal(-Combat.CultureMismatchDock, Combat.EquipmentScore(two));
        AssertScoreAppliesEquipment(two);
    }

    [Fact]
    public void Mixed_matching_and_mismatching_sums_tiers_then_one_mismatch()
    {
        var g = Kit(Man(Armatura.Murmillo),
            (EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2),
            (EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T3),
            (EquipmentSlot.Shield, EquipmentCulture.Roman, EquipmentTier.T1),
            (EquipmentSlot.Weapon, EquipmentCulture.Punic, EquipmentTier.T2));
        Assert.True(Combat.CultureMismatch(g));
        Assert.Equal(Combat.GearBonusCap - Combat.CultureMismatchDock, Combat.EquipmentScore(g));
        AssertScoreAppliesEquipment(g);
    }

    [Fact]
    public void Thraex_punic_is_exotic_ok_roman_mismatches()
    {
        var punic = Kit(Man(Armatura.Thraex),
            (EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T2),
            (EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T2));
        Assert.False(Combat.CultureMismatch(punic));
        Assert.Equal(2 * Combat.GearTier2Bonus, Combat.EquipmentScore(punic));

        var roman = Kit(Man(Armatura.Thraex),
            (EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T1));
        Assert.True(Combat.CultureMismatch(roman));
        Assert.Equal(-Combat.CultureMismatchDock, Combat.EquipmentScore(roman));
    }

    [Fact]
    public void Retiarius_greek_helm_mismatches_even_with_roman_weapon()
    {
        var g = Kit(Man(Armatura.Retiarius),
            (EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T1),
            (EquipmentSlot.Weapon, EquipmentCulture.Roman, EquipmentTier.T1));
        Assert.True(Combat.HasRomanWeapon(g));
        Assert.True(Combat.CultureMismatch(g));
        Assert.Equal(-Combat.CultureMismatchDock, Combat.EquipmentScore(g));
    }

    [Fact]
    public void Secutor_matches_roman_only()
    {
        var roman = Kit(Man(Armatura.Secutor),
            (EquipmentSlot.Helmet, EquipmentCulture.Roman, EquipmentTier.T2));
        Assert.False(Combat.CultureMismatch(roman));
        Assert.Equal(Combat.GearTier2Bonus, Combat.EquipmentScore(roman));

        var greek = Kit(Man(Armatura.Secutor),
            (EquipmentSlot.Armor, EquipmentCulture.Greek, EquipmentTier.T3));
        Assert.True(Combat.CultureMismatch(greek));
        Assert.Equal(Combat.GearTier3Bonus - Combat.CultureMismatchDock, Combat.EquipmentScore(greek));
    }

    [Fact]
    public void Score_can_skip_equipment_for_unknobbed_foes()
    {
        var retiariusFoe = Man(Armatura.Retiarius);
        var player = Man(Armatura.Secutor);
        int playerSide = Combat.Score(new Random(7), retiariusFoe, player, 16);
        int foeSide = Combat.Score(new Random(7), retiariusFoe, player, 16, applyEquipment: false);
        Assert.Equal(foeSide - Combat.RetiariusMissingRomanWeapon, playerSide);
        Assert.Equal(-Combat.RetiariusMissingRomanWeapon, Combat.EquipmentScore(retiariusFoe));
    }

    [Fact]
    public void Locatio_stans_docks_mismatched_kit_with_wrong_armatura_numbers()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        murmillo.Helmet = Piece(EquipmentSlot.Helmet, EquipmentCulture.Greek, EquipmentTier.T1, 99);
        var offer = new Contract { Requested = Armatura.Murmillo, PaySudore = 30, PayOccisus = 400 };
        s.Offer = offer;
        var settled = Ludus.SettleBout(s, new Random(1), StansBout(murmillo, wrongType: false, offer), IugulaChoice.Mitte, IugulaChoice.Mitte);
        Assert.Equal(Ludus.LocatioSudore(offer, wrong: true), settled.Pay);
        Assert.Equal(Math.Max(Ludus.LocatioSudoreFloor, 30 - Ludus.LocatioWrongSudoreDock), settled.Pay);
    }

    [Fact]
    public void Locatio_does_not_stack_wrong_armatura_and_mismatch()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        murmillo.Armor = Piece(EquipmentSlot.Armor, EquipmentCulture.Punic, EquipmentTier.T2, 5);
        var offer = new Contract { Requested = Armatura.Thraex, PaySudore = 36, PayOccisus = 420 };
        s.Offer = offer;
        var settled = Ludus.SettleBout(s, new Random(1), StansBout(murmillo, wrongType: true, offer), IugulaChoice.Mitte, IugulaChoice.Mitte);
        Assert.Equal(Ludus.LocatioSudore(offer, wrong: true), settled.Pay);
        Assert.NotEqual(Math.Max(Ludus.LocatioSudoreFloor, 36 - 2 * Ludus.LocatioWrongSudoreDock), settled.Pay);
    }

    [Fact]
    public void Locatio_thraex_punic_kit_does_not_dock()
    {
        var s = Fresh();
        var thraex = s.Living.Single(g => g.Armatura == Armatura.Thraex);
        thraex.Helmet = Piece(EquipmentSlot.Helmet, EquipmentCulture.Punic, EquipmentTier.T1, 3);
        var offer = new Contract { Requested = Armatura.Thraex, PaySudore = 30, PayOccisus = 400 };
        s.Offer = offer;
        var settled = Ludus.SettleBout(s, new Random(1), StansBout(thraex, wrongType: false, offer), IugulaChoice.Mitte, IugulaChoice.Mitte);
        Assert.Equal(Ludus.LocatioSudore(offer, wrong: false), settled.Pay);
        Assert.Equal(30, settled.Pay);
    }

    [Fact]
    public void Locatio_occisus_docks_mismatched_kit()
    {
        var s = Fresh();
        var murmillo = s.Living.Single(g => g.Armatura == Armatura.Murmillo);
        murmillo.Weapon = Piece(EquipmentSlot.Weapon, EquipmentCulture.Greek, EquipmentTier.T1, 8);
        var offer = new Contract { Requested = Armatura.Murmillo, PaySudore = 28, PayOccisus = 400 };
        s.Offer = offer;
        var bout = new Bout
        {
            Fighter = murmillo,
            Foe = Man(Armatura.Thraex),
            Report = new CombatReport { Outcome = FightOutcome.Missio, PlayerDown = true },
            Hosted = false,
            WrongType = false,
            Offer = offer
        };
        var settled = Ludus.SettleBout(s, new Random(1), bout, IugulaChoice.Iugula, IugulaChoice.Mitte);
        Assert.True(settled.OwnDied);
        Assert.Equal(Ludus.LocatioOccisus(offer, wrong: true), settled.Pay);
        Assert.Equal(Math.Max(Ludus.LocatioOccisusFloor, 400 - Ludus.LocatioWrongOccisusDock), settled.Pay);
    }

    static Bout StansBout(Gladiator g, bool wrongType, Contract offer) => new()
    {
        Fighter = g,
        Foe = Man(Armatura.Thraex),
        Report = new CombatReport { Outcome = FightOutcome.Stans },
        Hosted = false,
        WrongType = wrongType,
        Offer = offer
    };
}
