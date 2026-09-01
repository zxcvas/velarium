namespace Velarium;

public sealed class CareerStats
{
    public int Seed { get; init; }
    public int Days { get; set; }
    public int Denarii { get; set; }
    public int Fama { get; set; }
    public int Bouts { get; set; }
    public int OwnDeaths { get; set; }
    public int FoeDeaths { get; set; }
    public int SudoreCount { get; set; }
    public int OccisusCount { get; set; }
    public bool Ruined { get; set; }
    public bool HostingUnlockedBy30 { get; set; }
    public bool StaffedKitchenBy30 { get; set; }
    public int? DenariiDay1 { get; set; }
    public int? DenariiDay7 { get; set; }
    public int? DenariiDay12 { get; set; }
    public int? DenariiDay30 { get; set; }
}

public sealed class AggregateReport
{
    public int Careers { get; init; }
    public int Ruined { get; init; }
    public double MeanDays { get; init; }
    public double MeanFama { get; init; }
    public int Bouts { get; init; }
    public int OwnDeaths { get; init; }
    public int FoeDeaths { get; init; }
    public int SudoreCount { get; init; }
    public int OccisusCount { get; init; }
    public int HostingUnlockedBy30 { get; init; }
    public int StaffedKitchenBy30 { get; init; }
    public double? MeanDenariiDay1 { get; init; }
    public double? MeanDenariiDay7 { get; init; }
    public double? MeanDenariiDay12 { get; init; }
    public double? MeanDenariiDay30 { get; init; }

    public double CombatantDeathRate => Bouts == 0 ? 0 : (OwnDeaths + FoeDeaths) / (2.0 * Bouts);
    public double OwnDeathRate => Bouts == 0 ? 0 : OwnDeaths / (double)Bouts;
    public double RuinedPct => Careers == 0 ? 0 : 100.0 * Ruined / Careers;
    public double HostingPct => Careers == 0 ? 0 : 100.0 * HostingUnlockedBy30 / Careers;
    public double KitchenPct => Careers == 0 ? 0 : 100.0 * StaffedKitchenBy30 / Careers;
}

public static class CareerSim
{
    public const int DefaultMaxDays = 60;

    public static CareerStats RunCareer(int seed, int maxDays = DefaultMaxDays)
    {
        var rng = new Random(seed);
        var s = Ludus.Start(rng, seed, "Lucius", "Atinius", "Strabo");
        var stats = new CareerStats { Seed = seed };

        while (!s.Ended && s.DaysPlayed < maxDays)
        {
            MorningTreat(s);
            MorningOrders(s);
            MorningKitchen(s);
            TryLocatio(s, rng, stats);

            var night = Ludus.EndDay(s, rng);
            if (night.Volunteer != null
                && s.Denarii > night.Bounty + 50
                && s.Living.Count() < 6)
            {
                Ludus.AcceptAuctoratus(s, night.Volunteer, night.Bounty);
            }

            Snapshot(s, stats);
            if (s.DaysPlayed <= 30 && s.HostingUnlocked)
                stats.HostingUnlockedBy30 = true;
            if (s.DaysPlayed <= 30 && Ludus.Staffed(s, RoomKind.Kitchen))
                stats.StaffedKitchenBy30 = true;
        }

        stats.Days = s.DaysPlayed;
        stats.Denarii = s.Denarii;
        stats.Fama = s.Fama;
        stats.Ruined = s.Ended;
        return stats;
    }

    public static AggregateReport RunMany(int n, int maxDays = DefaultMaxDays)
    {
        var list = new List<CareerStats>(n);
        for (int i = 1; i <= n; i++)
            list.Add(RunCareer(i, maxDays));
        return Aggregate(list);
    }

    public static AggregateReport Aggregate(IReadOnlyList<CareerStats> list)
    {
        int n = list.Count;
        return new AggregateReport
        {
            Careers = n,
            Ruined = list.Count(c => c.Ruined),
            MeanDays = n == 0 ? 0 : list.Average(c => c.Days),
            MeanFama = n == 0 ? 0 : list.Average(c => c.Fama),
            Bouts = list.Sum(c => c.Bouts),
            OwnDeaths = list.Sum(c => c.OwnDeaths),
            FoeDeaths = list.Sum(c => c.FoeDeaths),
            SudoreCount = list.Sum(c => c.SudoreCount),
            OccisusCount = list.Sum(c => c.OccisusCount),
            HostingUnlockedBy30 = list.Count(c => c.HostingUnlockedBy30),
            StaffedKitchenBy30 = list.Count(c => c.StaffedKitchenBy30),
            MeanDenariiDay1 = MeanNullable(list, c => c.DenariiDay1),
            MeanDenariiDay7 = MeanNullable(list, c => c.DenariiDay7),
            MeanDenariiDay12 = MeanNullable(list, c => c.DenariiDay12),
            MeanDenariiDay30 = MeanNullable(list, c => c.DenariiDay30)
        };
    }

    public static string Format(AggregateReport r)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("AMPHITEATER — headless career report");
        sb.AppendLine("Ville target: ~10% of combatants die per bout (deaths / (2 * bouts) ≈ 0.10).");
        sb.AppendLine("Gaius: sweat cheap, corpse dear. Start 620 denarii. Upkeep 10 + 6/mouth.");
        sb.AppendLine("AI: locatio-first, never hosts, mitte on own fallen (own-death stays 0).");
        sb.AppendLine("    Buys a cook if purse > 120 and kitchen empty. Night is rest.");
        sb.AppendLine();
        sb.AppendLine($"Careers:              {r.Careers}");
        sb.AppendLine($"Ruined:               {r.Ruined}  ({r.RuinedPct:0.0}%)");
        sb.AppendLine($"Mean days survived:   {r.MeanDays:0.0}");
        sb.AppendLine($"Mean fama (end):      {r.MeanFama:0.0}");
        sb.AppendLine($"Hosting unlocked ≤30: {r.HostingUnlockedBy30}  ({r.HostingPct:0.0}%)");
        sb.AppendLine($"Kitchen staffed ≤30:  {r.StaffedKitchenBy30}  ({r.KitchenPct:0.0}%)");
        sb.AppendLine();
        sb.AppendLine($"Bouts:                {r.Bouts}");
        sb.AppendLine($"Own deaths:           {r.OwnDeaths}");
        sb.AppendLine($"Foe deaths:           {r.FoeDeaths}");
        sb.AppendLine($"Combatant death rate: {r.CombatantDeathRate:0.000}   (Ville ~0.10)");
        sb.AppendLine($"Own-man death / bout: {r.OwnDeathRate:0.000}");
        sb.AppendLine($"Sudore settlements:   {r.SudoreCount}");
        sb.AppendLine($"Occisus settlements:  {r.OccisusCount}");
        sb.AppendLine();
        sb.AppendLine($"Mean denarii day 1:   {Fmt(r.MeanDenariiDay1)}");
        sb.AppendLine($"Mean denarii day 7:   {Fmt(r.MeanDenariiDay7)}");
        sb.AppendLine($"Mean denarii day 12:  {Fmt(r.MeanDenariiDay12)}");
        sb.AppendLine($"Mean denarii day 30:  {Fmt(r.MeanDenariiDay30)}");
        return sb.ToString();
    }

    static string Fmt(double? v) => v is null ? "—" : v.Value.ToString("0.0");

    static double? MeanNullable(IReadOnlyList<CareerStats> list, Func<CareerStats, int?> pick)
    {
        var vals = list.Select(pick).Where(v => v.HasValue).Select(v => v!.Value).ToList();
        if (vals.Count == 0) return null;
        return vals.Average();
    }

    static void MorningTreat(GameState s)
    {
        foreach (var g in s.Living.Where(Ludus.NeedsMedicus).ToList())
        {
            if (s.Denarii <= 30) break;
            Ludus.Treat(s, g);
        }
    }

    static void MorningOrders(GameState s)
    {
        foreach (var g in s.Living)
        {
            if (g.Status != GladiatorStatus.Validus)
                g.Order = DayOrder.Requies;
            else if (g.Pugnat == 0)
                g.Order = DayOrder.Palus;
            else
                g.Order = DayOrder.Sparring;
        }
    }

    const int CookPurseNeed = 120;
    const int CookCushion = 50;

    static void MorningKitchen(GameState s)
    {
        if (Ludus.Staffed(s, RoomKind.Kitchen)) return;
        if (s.Denarii <= CookPurseNeed) return;

        var idle = Ludus.LivingWorkers(s).FirstOrDefault(w =>
            w.CanWork && s.Rooms.All(r => r.AssignedWorkerId != w.Id));
        if (idle != null)
        {
            Ludus.AssignWorker(s, idle.Id, RoomKind.Kitchen);
            return;
        }

        if (s.LaborMarket.Count == 0) return;
        int best = 0;
        int bestPrice = int.MaxValue;
        for (int i = 0; i < s.LaborMarket.Count; i++)
        {
            int price = s.LaborMarket[i].Value();
            if (price < bestPrice)
            {
                bestPrice = price;
                best = i;
            }
        }
        if (s.Denarii < bestPrice + CookCushion) return;
        if (Ludus.BuyWorker(s, best) != null) return;
        var hired = Ludus.LivingWorkers(s).LastOrDefault(w => w.CanWork);
        if (hired != null)
            Ludus.AssignWorker(s, hired.Id, RoomKind.Kitchen);
    }

    static void TryLocatio(GameState s, Random rng, CareerStats stats)
    {
        if (s.OfferTakenToday || s.Offer == null) return;
        var able = s.Living.Where(g => g.CanFight).ToList();
        if (able.Count == 0) return;

        var requested = able.Where(g => g.Armatura == s.Offer.Requested).ToList();
        var pick = (requested.Count > 0 ? requested : able)
            .OrderByDescending(g => g.Virtus)
            .First();

        var bout = Ludus.RunBout(s, rng, pick, hosted: false);
        var settled = Ludus.SettleBout(s, rng, bout, IugulaChoice.Mitte, IugulaChoice.SimRolls);
        stats.Bouts++;
        if (settled.OwnDied) stats.OwnDeaths++;
        if (settled.FoeDied) stats.FoeDeaths++;
        if (settled.OccisusPay) stats.OccisusCount++;
        else stats.SudoreCount++;
    }

    static void Snapshot(GameState s, CareerStats stats)
    {
        switch (s.DaysPlayed)
        {
            case 1: stats.DenariiDay1 = s.Denarii; break;
            case 7: stats.DenariiDay7 = s.Denarii; break;
            case 12: stats.DenariiDay12 = s.Denarii; break;
            case 30: stats.DenariiDay30 = s.Denarii; break;
        }
    }
}
