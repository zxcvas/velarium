using Velarium;

namespace Amphiteater.Sim.Tests;

public class HouseTests
{
    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    [Fact]
    public void Start_has_courtyard_and_rival()
    {
        var s = Fresh();
        Assert.Equal(5, s.Rooms.Count);
        Assert.Equal(1, Ludus.RoomOf(s, RoomKind.Cellae)!.Level);
        Assert.Equal(0, Ludus.RoomOf(s, RoomKind.Medicus)!.Level);
        Assert.Equal(Ludus.CellCap, Ludus.Beds(s));
        Assert.NotNull(s.Rival);
        Assert.False(string.IsNullOrWhiteSpace(s.Rival!.Name));
        Assert.Empty(Ludus.LivingWorkers(s));
    }

    [Fact]
    public void Staffed_kitchen_cuts_fighter_upkeep()
    {
        var s = Fresh();
        var rng = new Random(2);
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        int withIdleHands = Ludus.Upkeep(s);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        int staffed = Ludus.Upkeep(s);
        Assert.True(staffed < withIdleHands);
        Assert.True(Ludus.Staffed(s, RoomKind.Kitchen));
    }

    [Fact]
    public void Cellae_upgrade_adds_beds_at_dusk()
    {
        var rng = new Random(3);
        var s = Ludus.Start(rng, 3, "Lucius", "Atinius", "Strabo");
        s.Denarii = 5_000;
        Assert.Equal(8, Ludus.Beds(s));
        Assert.Null(Ludus.BeginUpgrade(s, RoomKind.Cellae));
        Ludus.EndDay(s, rng);
        Assert.Equal(2, Ludus.RoomOf(s, RoomKind.Cellae)!.Level);
        Assert.Equal(10, Ludus.Beds(s));
    }

    [Fact]
    public void Poison_marks_the_next_locatio_foe()
    {
        var rng = new Random(4);
        var s = Ludus.Start(rng, 4, "Lucius", "Atinius", "Strabo");
        Assert.NotNull(s.Offer);
        s.Rival!.NextFoePoisoned = true;
        var g = s.Living.First(x => x.CanFight);
        var bout = Ludus.RunBout(s, rng, g, hosted: false);
        Assert.Equal(GladiatorStatus.Vulneratus, bout.Foe.Status);
        Assert.False(s.Rival.NextFoePoisoned);
    }

    [Fact]
    public void Kitchen_level_two_staffed_pays_stall()
    {
        var rng = new Random(5);
        var s = Ludus.Start(rng, 5, "Lucius", "Atinius", "Strabo");
        Ludus.RoomOf(s, RoomKind.Kitchen)!.Level = 2;
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        foreach (var g in s.Living) g.Order = DayOrder.None;
        var night = Ludus.EndDay(s, rng);
        Assert.Contains(night.Log, line => line.Contains("Thermopolium"));
        Assert.True(night.StallProfit > 0);
    }

    [Fact]
    public void Kitchen_level_two_unstaffed_has_no_stall()
    {
        var rng = new Random(6);
        var s = Ludus.Start(rng, 6, "Lucius", "Atinius", "Strabo");
        Ludus.RoomOf(s, RoomKind.Kitchen)!.Level = 2;
        foreach (var g in s.Living) g.Order = DayOrder.None;
        var night = Ludus.EndDay(s, rng);
        Assert.DoesNotContain(night.Log, line => line.Contains("Thermopolium"));
        Assert.Equal(0, night.StallProfit);
    }

    [Fact]
    public void Kitchen_level_one_has_no_stall()
    {
        var rng = new Random(7);
        var s = Ludus.Start(rng, 7, "Lucius", "Atinius", "Strabo");
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        foreach (var g in s.Living) g.Order = DayOrder.None;
        var night = Ludus.EndDay(s, rng);
        Assert.DoesNotContain(night.Log, line => line.Contains("Thermopolium"));
        Assert.Equal(0, night.StallProfit);
    }

    [Fact]
    public void Kitchen_level_three_uses_dish_margin()
    {
        var rng = new Random(8);
        var s = Ludus.Start(rng, 8, "Lucius", "Atinius", "Strabo");
        s.FoodRumorActive = false;
        Ludus.RoomOf(s, RoomKind.Kitchen)!.Level = 3;
        s.StallDish = DishKind.Moretum;
        var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
        s.Household.Add(w);
        Assert.Null(Ludus.AssignWorker(s, w.Id, RoomKind.Kitchen));
        var (cost, sale, nom) = Ludus.StallPrices(s);
        Assert.Equal(2, cost);
        Assert.Equal(6, sale);
        Assert.Contains("moretum", nom, StringComparison.OrdinalIgnoreCase);
        int clients = Ludus.StallClients(s);
        Assert.True(clients > 0);
        foreach (var g in s.Living) g.Order = DayOrder.None;
        var night = Ludus.EndDay(s, rng);
        Assert.Contains(night.Log, line => line.Contains("Thermopolium") && line.Contains("moretum"));
        Assert.True(night.StallProfit > 0);
    }

    [Fact]
    public void BuyWorker_fails_when_household_is_full()
    {
        var s = Fresh();
        while (Ludus.LivingWorkers(s).Count() < Ludus.HouseholdCap)
        {
            s.Household.Add(new Worker
            {
                Id = s.NextId++,
                Name = "H" + s.NextId,
                Alive = true,
                Vigor = 8,
                VigorMax = 8
            });
        }
        s.LaborMarket.Add(new Worker { Name = "EXTRA", VigorMax = 10, Vigor = 10, Alive = true });
        s.Denarii = 10_000;
        Assert.Equal("full", Ludus.BuyWorker(s, 0));
    }

    [Fact]
    public void Spy_armatura_intel_seeds_tomorrows_locatio()
    {
        var rng = new Random(11);
        var s = Ludus.Start(rng, 11, "Lucius", "Atinius", "Strabo");
        string report = Ludus.GrantSpyIntel(s, rng, SpyIntelKind.TomorrowArmatura);
        Assert.Equal(SpyIntelKind.TomorrowArmatura, s.Rival!.IntelKind);
        Assert.True(s.Rival.HasSeededOffer);
        Assert.Contains(Content.ArmaturaNom(s.Rival.SeededArmatura), report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("star murmillo", report, StringComparison.OrdinalIgnoreCase);

        var kit = s.Rival.SeededArmatura;
        int sudore = s.Rival.SeededSudore;
        int occisus = s.Rival.SeededOccisus;
        s.DaysPlayed = 8;
        Ludus.RefreshOffer(s, rng);
        Assert.NotNull(s.Offer);
        Assert.Equal(kit, s.Offer!.Requested);
        Assert.Equal(sudore, s.Offer.PaySudore);
        Assert.Equal(occisus, s.Offer.PayOccisus);
        Assert.False(s.Rival.HasSeededOffer);
    }

    [Fact]
    public void Spy_purse_intel_lifts_coin_and_raises_tomorrows_pay()
    {
        var rng = new Random(12);
        var s = Ludus.Start(rng, 12, "Lucius", "Atinius", "Strabo");
        int purse = s.Denarii;
        string report = Ludus.GrantSpyIntel(s, rng, SpyIntelKind.PurseThin);
        Assert.InRange(s.Denarii - purse, Ludus.SpyPurseLiftMin, Ludus.SpyPurseLiftMax - 1);
        Assert.Contains("lift", report, StringComparison.OrdinalIgnoreCase);
        Assert.InRange(s.Rival!.SeededSudore, Ludus.SpyHighSudoreMin, Ludus.SpyHighSudoreMax - 1);

        int sudore = s.Rival.SeededSudore;
        s.DaysPlayed = 8;
        Ludus.RefreshOffer(s, rng);
        Assert.NotNull(s.Offer);
        Assert.Equal(sudore, s.Offer!.PaySudore);
        Assert.InRange(s.Offer.PaySudore, Ludus.SpyHighSudoreMin, Ludus.SpyHighSudoreMax - 1);
    }

    [Fact]
    public void Spy_miss_intel_forces_tomorrows_editor()
    {
        var rng = new Random(13);
        var s = Ludus.Start(rng, 13, "Lucius", "Atinius", "Strabo");
        string report = Ludus.GrantSpyIntel(s, rng, SpyIntelKind.RivalMisses);
        Assert.True(s.Rival!.MissTomorrow);
        Assert.Contains("The contract will come", report);

        s.DaysPlayed = 8;
        Ludus.RefreshOffer(s, rng);
        Assert.NotNull(s.Offer);
        Assert.False(s.Rival.MissTomorrow);
    }

    [Fact]
    public void Spy_roster_intel_sends_the_next_foe_in_already_off()
    {
        var rng = new Random(14);
        var s = Ludus.Start(rng, 14, "Lucius", "Atinius", "Strabo");
        Assert.NotNull(s.Offer);
        string report = Ludus.GrantSpyIntel(s, rng, SpyIntelKind.WeakRoster);
        Assert.True(s.Rival!.NextFoeWeak);
        Assert.Contains("fessus", report, StringComparison.OrdinalIgnoreCase);

        var g = s.Living.First(x => x.CanFight);
        var bout = Ludus.RunBout(s, rng, g, hosted: false);
        Assert.Equal(GladiatorStatus.Fessus, bout.Foe.Status);
        Assert.False(s.Rival.NextFoeWeak);
        Assert.True(bout.Foe.Vigor < bout.Foe.VigorMax);
    }

    [Fact]
    public void Poison_still_outranks_spy_weak_foe()
    {
        var rng = new Random(15);
        var s = Ludus.Start(rng, 15, "Lucius", "Atinius", "Strabo");
        Ludus.GrantSpyIntel(s, rng, SpyIntelKind.WeakRoster);
        s.Rival!.NextFoePoisoned = true;
        var g = s.Living.First(x => x.CanFight);
        var bout = Ludus.RunBout(s, rng, g, hosted: false);
        Assert.Equal(GladiatorStatus.Vulneratus, bout.Foe.Status);
        Assert.False(s.Rival.NextFoePoisoned);
        Assert.False(s.Rival.NextFoeWeak);
    }

    [Fact]
    public void Spy_success_through_EndDay_is_on_the_dawn_offer()
    {
        var rng = new Random(16);
        var s = Ludus.Start(rng, 16, "Lucius", "Atinius", "Strabo");
        foreach (var g in s.Living) g.Order = DayOrder.None;
        Ludus.GrantSpyIntel(s, rng, SpyIntelKind.TomorrowArmatura);
        var kit = s.Rival!.SeededArmatura;
        var night = Ludus.EndDay(s, rng);
        Assert.NotNull(s.Offer);
        Assert.Equal(kit, s.Offer!.Requested);
        Assert.Contains(Content.ArmaturaNom(kit), s.Rival.Intel, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(night.Log, line => line.Contains("star murmillo", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ResolveNightOps_spy_success_is_never_empty_flavor()
    {
        bool sawSuccess = false;
        bool sawMiss = false;
        bool sawCaught = false;
        for (int seed = 1; seed <= 400 && !(sawSuccess && sawMiss && sawCaught); seed++)
        {
            var rng = new Random(seed);
            var s = Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
            var w = Ludus.MakeWorker(s, rng, new HashSet<string>());
            s.Household.Add(w);
            s.NightOrder = NightOrder.Spy;
            s.NightActorIsWorker = true;
            s.NightActorId = w.Id;
            var log = Ludus.ResolveNightOps(s, rng);
            string blob = string.Join(" ", log);
            if (blob.Contains("taken at the gate", StringComparison.OrdinalIgnoreCase))
            {
                sawCaught = true;
                Assert.True(s.Rival!.Hostility >= 2);
                Assert.True(string.IsNullOrEmpty(s.Rival.Intel) || s.Rival.IntelKind == SpyIntelKind.None);
                continue;
            }
            if (blob.Contains("Dogs, a barred porta", StringComparison.OrdinalIgnoreCase))
            {
                sawMiss = true;
                Assert.Equal(SpyIntelKind.None, s.Rival!.IntelKind);
                continue;
            }
            sawSuccess = true;
            Assert.NotEqual(SpyIntelKind.None, s.Rival!.IntelKind);
            Assert.False(string.IsNullOrWhiteSpace(s.Rival.Intel));
            Assert.DoesNotContain("star murmillo", s.Rival.Intel, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(log, line => line.Contains(s.Rival.Intel));
            Assert.True(
                s.Rival.HasSeededOffer
                || s.Rival.SeededSudore > 0
                || s.Rival.MissTomorrow
                || s.Rival.NextFoeWeak);
        }

        Assert.True(sawSuccess, "expected at least one spy success");
        Assert.True(sawMiss, "expected the dogs / barred porta miss to remain");
        Assert.True(sawCaught, "expected a caught spy");
    }
}
