using Velarium;

namespace Amphiteater.Sim.Tests;

public class LudusTests
{
    static GameState Fresh(int seed = 1)
    {
        var rng = new Random(seed);
        return Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
    }

    [Fact]
    public void Start_has_purse_three_tiros_and_kalends()
    {
        var s = Fresh();
        Assert.Equal(Ludus.StartDenarii, s.Denarii);
        Assert.Equal(3, s.Living.Count());
        Assert.Equal(Ludus.StartFama, s.Fama);
        Assert.Equal(Ludus.StartYearAuc, s.YearAuc);
        Assert.Equal(Ludus.StartMonth, s.Month);
        Assert.Equal(Ludus.StartDay, s.Day);
        Assert.Equal("Kalendis Maiis, a.u.c. DCCLXXXII", Calendar.Format(s));
        Assert.All(s.Living, g => Assert.Equal(0, g.Pugnat));
    }

    [Fact]
    public void EndDay_idle_three_mouths_logs_twenty_eight_upkeep()
    {
        var rng = new Random(1);
        var s = Ludus.Start(rng, 1, "Lucius", "Atinius", "Strabo");
        foreach (var g in s.Living)
            g.Order = DayOrder.None;
        var night = Ludus.EndDay(s, rng);
        Assert.Equal(28, Ludus.Upkeep(3));
        Assert.Contains(night.Log, line => line.Contains("-28 denarii"));
        Assert.Equal(1, s.DaysPlayed);
    }

    [Fact]
    public void Treat_deducts_fee_and_raises_vigor()
    {
        var s = Fresh();
        var g = s.Living.First();
        g.Vigor = 4;
        g.Status = GladiatorStatus.Vulneratus;
        int purse = s.Denarii;
        Assert.True(Ludus.Treat(s, g));
        Assert.Equal(purse - Ludus.MedicusFee, s.Denarii);
        Assert.True(g.Vigor > 4);
    }

    [Fact]
    public void Buy_fails_when_cells_are_full()
    {
        var s = Fresh();
        while (s.Living.Count() < Ludus.CellCap)
        {
            s.Familia.Add(new Gladiator
            {
                Id = s.NextId++,
                Name = "FILL" + s.NextId,
                Status = GladiatorStatus.Validus,
                Vigor = 10,
                VigorMax = 10
            });
        }
        s.Market.Clear();
        s.Market.Add(new Gladiator
        {
            Name = "EXTRA",
            Status = GladiatorStatus.Validus,
            Vigor = 10,
            VigorMax = 10,
            Virtus = 5
        });
        s.Denarii = 10_000;
        Assert.Equal("full", Ludus.Buy(s, 0));
        Assert.Equal(Ludus.CellCap, s.Living.Count());
    }

    [Fact]
    public void RunCareer_seed_1_is_deterministic()
    {
        var a = CareerSim.RunCareer(1, maxDays: 5);
        var b = CareerSim.RunCareer(1, maxDays: 5);
        Assert.Equal(a.Days, b.Days);
        Assert.Equal(a.Denarii, b.Denarii);
        Assert.Equal(a.Fama, b.Fama);
        Assert.Equal(a.Bouts, b.Bouts);
        Assert.Equal(a.OwnDeaths, b.OwnDeaths);
        Assert.Equal(a.FoeDeaths, b.FoeDeaths);
        Assert.Equal(a.SudoreCount, b.SudoreCount);
        Assert.Equal(a.OccisusCount, b.OccisusCount);
        Assert.Equal(a.Ruined, b.Ruined);
    }

    [Fact]
    public void RunCareer_smoke_twenty_by_five_days()
    {
        var r = CareerSim.RunMany(20, maxDays: 5);
        Assert.Equal(20, r.Careers);
        Assert.InRange(r.CombatantDeathRate, 0, 1);
        Assert.True(r.Bouts >= 0);
        Assert.True(r.RuinedPct < 100);
    }
}
