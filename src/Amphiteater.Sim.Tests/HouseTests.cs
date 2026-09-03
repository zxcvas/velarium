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
}
