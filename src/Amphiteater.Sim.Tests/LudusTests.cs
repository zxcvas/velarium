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
    public void CheckEnd_empty_purse_closes_ludus_even_with_men()
    {
        var s = Fresh();
        Assert.True(s.Living.Any());
        s.Denarii = 0;
        Ludus.CheckEnd(s);
        Assert.True(s.Ended);
        Assert.Equal("Ludus clausus", s.EndTitle);
    }

    [Fact]
    public void EndDay_overspend_closes_at_dusk()
    {
        var rng = new Random(1);
        var s = Ludus.Start(rng, 1, "Lucius", "Atinius", "Strabo");
        foreach (var g in s.Living)
            g.Order = DayOrder.None;
        s.Denarii = 5;
        Ludus.EndDay(s, rng);
        Assert.True(s.Ended);
        Assert.Equal(0, s.Denarii);
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
    public void Two_hosted_bouts_charge_the_gate_once()
    {
        var rng = new Random(9);
        var s = Ludus.Start(rng, 9, "Lucius", "Atinius", "Strabo");
        s.Fama = 20;
        foreach (var g in s.Living)
        {
            g.Palmae = 1;
            g.Vigor = g.VigorMax;
            g.Status = GladiatorStatus.Validus;
        }
        int purse = s.Denarii;
        Assert.True(Ludus.TryPayHost(s));
        Assert.Equal(purse - Ludus.HostCost, s.Denarii);
        var first = s.Living.First(g => g.CanFight);
        var b1 = Ludus.RunBout(s, rng, first, hosted: true);
        Ludus.SettleBout(s, rng, b1, IugulaChoice.Mitte, IugulaChoice.Mitte);
        var second = s.Living.First(g => g.CanFight && g.Id != first.Id);
        var b2 = Ludus.RunBout(s, rng, second, hosted: true);
        Ludus.SettleBout(s, rng, b2, IugulaChoice.Mitte, IugulaChoice.Mitte);
        Assert.True(s.HasHosted);
        Assert.True(s.Denarii >= purse - Ludus.HostCost);
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

    [Fact]
    public void GrantRudis_frees_a_star_pays_and_raises_fama()
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
        int cost = Ludus.RudisCost(g);
        int purse = s.Denarii;
        int mouths = s.Living.Count();
        Assert.Null(Ludus.RudisRefusal(s, g));
        Assert.Null(Ludus.GrantRudis(s, g));
        Assert.DoesNotContain(s.Familia, x => x.Id == g.Id);
        Assert.Equal(mouths - 1, s.Living.Count());
        Assert.Equal(purse - cost, s.Denarii);
        Assert.Equal(Ludus.RudisFamaNeed + Ludus.RudisFamaGain, s.Fama);
        Assert.Contains(s.Rudiarii, line => line.Contains(g.Name) && line.Contains("rudiarius"));
        Assert.DoesNotContain(s.AdLibitinam, line => line.Contains(g.Name));
        Assert.False(s.Ended);
    }

    [Fact]
    public void GrantRudis_rejects_tiro_without_palmae()
    {
        var s = Fresh();
        s.Fama = 50;
        s.Denarii = 5_000;
        var g = s.Living.First();
        Assert.Equal(0, g.Palmae);
        Assert.Equal("palmae", Ludus.GrantRudis(s, g));
        Assert.Contains(s.Living, x => x.Id == g.Id);
        Assert.Empty(s.Rudiarii);
    }

    [Fact]
    public void GrantRudis_rejects_low_ludus_fama()
    {
        var s = Fresh();
        s.Fama = Ludus.RudisFamaNeed - 1;
        s.Denarii = 5_000;
        var g = s.Living.First();
        g.Palmae = Ludus.RudisPalmaeNeed;
        Assert.Equal("fama", Ludus.GrantRudis(s, g));
        Assert.Contains(s.Living, x => x.Id == g.Id);
        Assert.Equal(Ludus.RudisFamaNeed - 1, s.Fama);
    }

    [Fact]
    public void GrantRudis_rejects_short_purse()
    {
        var s = Fresh();
        s.Fama = Ludus.RudisFamaNeed;
        var g = s.Living.First();
        g.Palmae = Ludus.RudisPalmaeNeed;
        s.Denarii = Ludus.RudisCost(g) - 1;
        Assert.Equal("coin", Ludus.GrantRudis(s, g));
        Assert.Contains(s.Living, x => x.Id == g.Id);
        Assert.Equal(Ludus.RudisCost(g) - 1, s.Denarii);
    }

    [Fact]
    public void GrantRudis_rejects_dead_or_stranger()
    {
        var s = Fresh();
        s.Fama = 50;
        s.Denarii = 5_000;
        var g = s.Living.First();
        g.Palmae = 8;
        Ludus.Kill(s, g);
        Assert.Equal("gone", Ludus.GrantRudis(s, g));

        var stranger = new Gladiator
        {
            Name = "NEMO",
            Palmae = 8,
            Status = GladiatorStatus.Validus,
            Vigor = 10,
            VigorMax = 10
        };
        Assert.Equal("gone", Ludus.GrantRudis(s, stranger));
        Assert.Empty(s.Rudiarii);
    }

    [Fact]
    public void RudisCost_is_two_thirds_value_floored_at_min()
    {
        var cheap = new Gladiator
        {
            Virtus = 1,
            Palmae = 0,
            Fama = 0,
            VigorMax = 1,
            Status = GladiatorStatus.Validus
        };
        Assert.Equal(Ludus.RudisMinCost, Ludus.RudisCost(cheap));

        var star = new Gladiator
        {
            Virtus = 10,
            Palmae = 5,
            Fama = 4,
            VigorMax = 16,
            Status = GladiatorStatus.Validus
        };
        Assert.Equal(Math.Max(Ludus.RudisMinCost, star.Value() * 2 / 3), Ludus.RudisCost(star));
        Assert.True(Ludus.RudisCost(star) >= Ludus.RudisMinCost);
    }

    [Fact]
    public void GrantRudis_last_man_does_not_close_until_check_end()
    {
        var s = Fresh();
        s.Fama = 50;
        s.Denarii = 5_000;
        var keep = s.Living.First();
        keep.Palmae = 8;
        foreach (var g in s.Living.Where(x => x.Id != keep.Id).ToList())
            Ludus.Kill(s, g);
        Assert.Null(Ludus.GrantRudis(s, keep));
        Assert.Empty(s.Living);
        Assert.False(s.Ended);
        Ludus.CheckEnd(s);
        Assert.True(s.Ended);
    }

    [Fact]
    public void GrantRudis_clears_night_blade_if_he_was_the_actor()
    {
        var s = Fresh();
        s.Fama = 50;
        s.Denarii = 5_000;
        var g = s.Living.First();
        g.Palmae = 8;
        s.NightOrder = NightOrder.Spy;
        s.NightActorIsWorker = false;
        s.NightActorId = g.Id;
        Assert.Null(Ludus.GrantRudis(s, g));
        Assert.Equal(NightOrder.Rest, s.NightOrder);
        Assert.Equal(0, s.NightActorId);
    }

    [Fact]
    public void RunCareer_some_careers_staff_kitchen_by_day_twelve()
    {
        var r = CareerSim.RunMany(20, maxDays: 12);
        Assert.True(r.StaffedKitchenBy30 > 0, "locatio-first AI should hire a cook in at least one of 20 careers");
    }
}
