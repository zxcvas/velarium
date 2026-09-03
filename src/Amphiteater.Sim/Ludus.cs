namespace Velarium;

public enum IugulaChoice
{
    SimRolls,
    Mitte,
    Iugula
}

public sealed class Bout
{
    public Gladiator Fighter { get; init; } = null!;
    public Gladiator Foe { get; init; } = null!;
    public CombatReport Report { get; init; } = null!;
    public bool Hosted { get; init; }
    public bool WrongType { get; init; }
    public Contract? Offer { get; init; }
}

public sealed class MunusSettlement
{
    public int Pay { get; init; }
    public int FamaDelta { get; init; }
    public bool OwnDied { get; init; }
    public bool FoeDied { get; init; }
    public bool OccisusPay { get; init; }
    public bool HostingJustUnlocked { get; init; }
    public List<string> Lines { get; init; } = new();
}

public sealed class EndDayResult
{
    public List<string> Log { get; init; } = new();
    public Gladiator? Volunteer { get; init; }
    public int Bounty { get; init; }
}

public static partial class Ludus
{
    public const int StartDenarii = 620;
    public const int StartFama = 3;
    public const int StartYearAuc = 782;
    public const int StartMonth = 5;
    public const int StartDay = 1;
    public const int HostCost = 220;
    public const int MedicusFee = 15;
    public const int CellCap = 8;
    public const int UpkeepRoof = 10;
    public const int UpkeepPerMouth = 6;
    public const int WorkerUpkeep = 3;
    public const int HouseholdCap = 6;
    public const int HostFamaNeed = 16;
    public const int MaxRoomLevel = 3;

    public static int Upkeep(int mouths) => UpkeepRoof + mouths * UpkeepPerMouth;

    public static GameState Start(Random rng, int seed, string pra, string nom, string cog)
    {
        var s = new GameState
        {
            Seed = seed,
            YearAuc = StartYearAuc,
            Month = StartMonth,
            Day = StartDay,
            Praenomen = pra,
            Nomen = nom,
            Cognomen = cog,
            LudusName = "Ludus " + nom,
            Denarii = StartDenarii,
            Fama = StartFama,
            NextId = 1
        };

        var taken = new HashSet<string>();
        Armatura[] kit = { Armatura.Murmillo, Armatura.Thraex, Armatura.Retiarius };
        foreach (var a in kit)
            s.Familia.Add(MakeGladiator(s, rng, taken, a, tiro: true));

        RefreshMarket(s, rng);
        EnsureHouse(s, rng);
        RefreshOffer(s, rng);
        return s;
    }

    public static Gladiator MakeGladiator(GameState s, Random rng, HashSet<string> taken, Armatura a, bool tiro)
    {
        string name = Content.UniqueName(rng, taken);
        taken.Add(name);
        var g = new Gladiator
        {
            Id = s.NextId++,
            Name = name,
            Origin = Content.Origins[rng.Next(Content.Origins.Length)],
            Armatura = a,
            VigorMax = rng.Next(14, 19),
            Virtus = tiro ? rng.Next(4, 8) : rng.Next(7, 12),
            Fama = tiro ? 0 : rng.Next(0, 4),
            Palmae = tiro ? 0 : rng.Next(0, 3),
            Pugnat = tiro ? 0 : rng.Next(1, 6),
            Status = GladiatorStatus.Validus,
            Source = rng.Next(8) == 0 ? "auctoratus" : "servus"
        };
        g.Vigor = g.VigorMax;
        if (!tiro && g.Palmae > g.Pugnat) g.Pugnat = g.Palmae + rng.Next(0, 3);
        return g;
    }

    public static Armatura RandomArmatura(Random rng) => (Armatura)rng.Next(4);

    public static void RefreshMarket(GameState s, Random rng)
    {
        s.Market.Clear();
        int n = rng.Next(1, 3);
        var taken = new HashSet<string>(s.Familia.Select(g => g.Name));
        for (int i = 0; i < n; i++)
            s.Market.Add(MakeGladiator(s, rng, taken, RandomArmatura(rng), tiro: rng.Next(100) < 60));
    }

    public static void RefreshOffer(GameState s, Random rng)
    {
        bool rivalMissed = s.Rival is { MissTomorrow: true };
        if (s.Rival != null) s.Rival.MissTomorrow = false;

        // First two mornings always bring an editor so the core loop is visible.
        if (!rivalMissed && s.DaysPlayed >= 2 && rng.Next(100) < 32)
        {
            s.Offer = null;
            return;
        }
        var req = RandomArmatura(rng);
        int sudore = rng.Next(22, 38);
        int occisus = rng.Next(360, 520);
        s.Offer = new Contract
        {
            EditorName = Content.EditorNames[rng.Next(Content.EditorNames.Length)],
            EditorOffice = Content.EditorOffices[rng.Next(Content.EditorOffices.Length)],
            Venue = Content.Venues[rng.Next(Content.Venues.Length)],
            Requested = req,
            PaySudore = sudore,
            PayOccisus = occisus,
            RivalLanista = s.Rival?.Name ?? Content.RivalLanistae[rng.Next(Content.RivalLanistae.Length)]
        };
    }

    public static bool NeedsMedicus(Gladiator g)
        => g.Alive && (g.Status is GladiatorStatus.Vulneratus or GladiatorStatus.Aeger || g.Vigor < g.VigorMax);

    public static bool Treat(GameState s, Gladiator g)
    {
        int fee = TreatFee(s);
        if (s.Denarii < fee) return false;
        s.Denarii -= fee;
        int heal = 6 + (Staffed(s, RoomKind.Medicus) ? RoomOf(s, RoomKind.Medicus)!.Level : 0);
        g.Vigor = Math.Min(g.VigorMax, g.Vigor + heal);
        if (g.Status is GladiatorStatus.Vulneratus or GladiatorStatus.Aeger)
            g.Status = g.Vigor >= g.VigorMax - 2 ? GladiatorStatus.Validus : GladiatorStatus.Fessus;
        return true;
    }

    public static string? Buy(GameState s, int marketIndex)
    {
        if (marketIndex < 0 || marketIndex >= s.Market.Count) return "gone";
        if (s.Living.Count() >= Beds(s)) return "full";
        var g = s.Market[marketIndex];
        int price = g.Value();
        if (s.Denarii < price) return "coin";
        s.Denarii -= price;
        g.Id = s.NextId++;
        g.Order = DayOrder.None;
        s.Familia.Add(g);
        s.Market.RemoveAt(marketIndex);
        return null;
    }

    public static void DeclineOfferNoFighter(GameState s)
    {
        s.Fama = Math.Max(0, s.Fama - 1);
        s.OfferTakenToday = true;
    }

    public static bool TryPayHost(GameState s)
    {
        if (s.Denarii < HostCost) return false;
        s.Denarii -= HostCost;
        return true;
    }

    public static void RefundHost(GameState s) => s.Denarii += HostCost;

    public static Bout RunBout(GameState s, Random rng, Gladiator g, bool hosted)
    {
        Armatura foeType = hosted
            ? Content.ClassicFoe(g.Armatura)
            : (rng.Next(100) < 80 ? s.Offer!.Requested : Content.ClassicFoe(g.Armatura));
        if (!hosted && s.Offer != null)
            foeType = Content.ClassicFoe(s.Offer.Requested);

        var foe = Combat.MakeFoe(rng, foeType, s.DaysPlayed);
        if (!hosted && s.Offer != null && rng.Next(100) < 70)
            foe.Armatura = Content.ClassicFoe(s.Offer.Requested);
        if (!hosted && s.Rival is { NextFoePoisoned: true })
        {
            foe.Status = GladiatorStatus.Vulneratus;
            foe.Virtus = Math.Max(3, foe.Virtus - 2);
            foe.Vigor = Math.Max(4, foe.VigorMax * 2 / 3);
            s.Rival.NextFoePoisoned = false;
        }

        var report = Combat.Fight(rng, g, foe);

        g.Pugnat++;
        g.FoughtToday = true;
        g.Vigor = Math.Max(1, report.PlayerVigorAfter);
        if (g.Vigor < g.VigorMax / 2) g.Status = GladiatorStatus.Fessus;

        return new Bout
        {
            Fighter = g,
            Foe = foe,
            Report = report,
            Hosted = hosted,
            WrongType = !hosted && s.Offer != null && g.Armatura != s.Offer.Requested,
            Offer = s.Offer
        };
    }

    public static MunusSettlement SettleBout(GameState s, Random rng, Bout bout, IugulaChoice ownFallen, IugulaChoice foeFallen)
    {
        var g = bout.Fighter;
        var foe = bout.Foe;
        var report = bout.Report;
        bool hosted = bout.Hosted;
        bool wrongType = bout.WrongType;
        var lines = new List<string>();
        int pay = 0;
        int famaDelta = 0;
        bool ownDied = false;
        bool foeDied = false;
        bool occisusPay = false;

        if (report.Outcome == FightOutcome.Stans)
        {
            g.Stantes++;
            g.Virtus = Math.Min(18, g.Virtus + (rng.Next(2) == 0 ? 1 : 0));
            g.Fama++;
            pay = hosted ? rng.Next(90, 140) : Sudore(bout.Offer, wrongType);
            famaDelta = report.Spectacular ? 2 : 1;
            lines.Add($"Stans. Both leave the sand. The crowd is divided; the clerks are not. Pro sudore: {pay} denarii.");
        }
        else if (report.Outcome == FightOutcome.Victoria || report.Outcome == FightOutcome.VictoriaSineMissione)
        {
            bool killFoe = ResolveIugula(foeFallen, () => report.CrowdBloodlust >= 3 && rng.Next(100) < 40);
            foeDied = killFoe;
            if (!hosted)
            {
                lines.Add(killFoe
                    ? $"The crowd wants the sword. The editor does not lift his hand. {foe.Name} is finished."
                    : $"Shouts of mitte. The editor waves the wooden staff. {foe.Name} lives to be rented again.");
            }

            g.Palmae++;
            g.Virtus = Math.Min(18, g.Virtus + 1);
            g.Fama += killFoe ? 2 : 1;
            g.Vigor = Math.Max(g.Vigor, 3);
            pay = hosted
                ? rng.Next(160, 280) + (killFoe ? 40 : 0) + (report.Spectacular ? 30 : 0)
                : Sudore(bout.Offer, wrongType) + 25 + g.Palmae * 2;
            famaDelta = (killFoe ? 2 : 1) + (report.Spectacular ? 1 : 0) + (hosted ? 3 : 0);
            if (wrongType) famaDelta = Math.Max(0, famaDelta - 1);
            lines.Add($"{g.Name} takes the palma. {(hosted ? "Gate and gifts" : "The editor's purse")}: {pay} denarii.");
        }
        else
        {
            bool iugula = ResolveIugula(ownFallen, () =>
            {
                int chance = 25 + report.CrowdBloodlust * 10 - g.Fama * 4 - g.Palmae * 3;
                if (report.Spectacular) chance -= 10;
                return rng.Next(100) < Math.Clamp(chance, 8, 70);
            });

            if (!hosted)
            {
                lines.Add(iugula
                    ? "Iugula. The editor's hand turns. A man with a blade walks out from the porta libitinensis."
                    : "Mitte. The crowd has a use for him yet. He is dragged toward the gate of the living.");
            }

            if (iugula)
            {
                pay = hosted ? rng.Next(40, 90) : Occisus(bout.Offer, wrongType);
                famaDelta = hosted ? 1 : 0;
                occisusPay = !hosted;
                ownDied = true;
                lines.Add($"{g.Name} dies on the sand. {(hosted ? "The crowd has its death. Your purse has a hole." : $"Compensation for a destroyed man: {pay} denarii. You have sold more than sweat.")}");
                Kill(s, g);
            }
            else
            {
                g.Missiones++;
                g.Status = GladiatorStatus.Vulneratus;
                g.Vigor = Math.Max(1, g.VigorMax / 4);
                g.Virtus = Math.Min(18, g.Virtus + (rng.Next(3) == 0 ? 1 : 0));
                pay = hosted ? rng.Next(70, 120) : Sudore(bout.Offer, wrongType) * 2 / 3;
                famaDelta = report.Spectacular ? 1 : 0;
                lines.Add($"Missio. {g.Name} will eat barley on his back for a while. Pay: {pay} denarii.");
            }
        }

        s.Denarii += pay;
        s.Fama = Math.Clamp(s.Fama + famaDelta, 0, 99);
        s.OfferTakenToday = true;
        if (!hosted) s.Offer = null;

        bool hostingJustUnlocked = !s.HasHosted && s.HostingUnlocked;
        if (hosted) s.HasHosted = true;

        return new MunusSettlement
        {
            Pay = pay,
            FamaDelta = famaDelta,
            OwnDied = ownDied,
            FoeDied = foeDied,
            OccisusPay = occisusPay,
            HostingJustUnlocked = hostingJustUnlocked,
            Lines = lines
        };
    }

    static bool ResolveIugula(IugulaChoice choice, Func<bool> simRoll) => choice switch
    {
        IugulaChoice.Mitte => false,
        IugulaChoice.Iugula => true,
        _ => simRoll()
    };

    static int Sudore(Contract? offer, bool wrong)
        => Math.Max(12, (offer?.PaySudore ?? 28) - (wrong ? 8 : 0));

    static int Occisus(Contract? offer, bool wrong)
        => Math.Max(80, (offer?.PayOccisus ?? 420) - (wrong ? 40 : 0));

    public static void Kill(GameState s, Gladiator g)
    {
        g.Status = GladiatorStatus.Mortuus;
        g.Vigor = 0;
        s.AdLibitinam.Add($"{g.Name}, {Content.ArmaturaNom(g.Armatura)} {g.Origin}, {g.Record()}");
    }

    public static EndDayResult EndDay(GameState s, Random rng)
    {
        var log = new List<string>();

        foreach (var g in s.Living.ToList())
        {
            if (g.FoughtToday)
            {
                log.Add($"{g.Name} fought today; the palus goes unused.");
                g.Order = DayOrder.None;
                if (g.Status == GladiatorStatus.Validus) g.Status = GladiatorStatus.Fessus;
                continue;
            }
            switch (g.Order)
            {
                case DayOrder.Palus:
                    if (rng.Next(100) < 55 && g.Virtus < 18) { g.Virtus++; log.Add($"{g.Name} works the palus. Virtus grows."); }
                    else log.Add($"{g.Name} sweats at the stake. No sudden art.");
                    g.Vigor = Math.Max(1, g.Vigor - 1);
                    if (rng.Next(100) < 8)
                    {
                        g.Status = GladiatorStatus.Vulneratus;
                        g.Vigor = Math.Max(1, g.Vigor - 2);
                        log.Add($"A slip. {g.Name} is nicked. The doctor of the yard is not a medicus.");
                    }
                    break;
                case DayOrder.Sparring:
                    if (g.Virtus < 18)
                    {
                        int gain = rng.Next(100) < 25 ? 2 : 1;
                        g.Virtus = Math.Min(18, g.Virtus + gain);
                        log.Add($"{g.Name} spars with the rudis. The familia watches. Virtus +{gain}.");
                    }
                    if (rng.Next(100) < 16)
                    {
                        g.Status = GladiatorStatus.Vulneratus;
                        g.Vigor = Math.Max(1, g.Vigor - 3);
                        log.Add($"The wooden sword is still a sword. {g.Name} will not stand tomorrow.");
                    }
                    else
                    {
                        g.Vigor = Math.Max(1, g.Vigor - 2);
                    }
                    break;
                case DayOrder.Requies:
                    g.Vigor = Math.Min(g.VigorMax, g.Vigor + 4 + KitchenBonus(s));
                    if (g.Status is GladiatorStatus.Fessus or GladiatorStatus.Vulneratus or GladiatorStatus.Aeger)
                    {
                        if (rng.Next(100) < 55) g.Status = GladiatorStatus.Validus;
                    }
                    log.Add($"{g.Name} to requies. Barley, oil, sleep.");
                    break;
                default:
                    g.Vigor = Math.Min(g.VigorMax, g.Vigor + 1 + KitchenBonus(s));
                    break;
            }
            g.Order = DayOrder.None;
        }

        CompleteUpgrades(s, log);
        RecoverDetained(s, rng, log);

        int mouths = s.Living.Count();
        int hands = LivingWorkers(s).Count();
        int upkeep = Upkeep(s);
        s.Denarii -= upkeep;
        log.Add($"Cibaria and the ludus: -{upkeep} denarii ({mouths} fighters, {hands} household, roof).");

        if (s.Denarii < 0)
        {
            log.Add("The purse is empty. The hordearii eat thin. Men lose vigor.");
            foreach (var g in s.Living)
            {
                g.Vigor = Math.Max(1, g.Vigor - 2);
                if (g.Status == GladiatorStatus.Validus) g.Status = GladiatorStatus.Fessus;
            }
            if (s.Living.Any() && rng.Next(100) < 40)
            {
                var taken = s.Living.OrderBy(g => g.Value()).First();
                log.Add($"A creditor takes {taken.Name} in lieu of coin. The cell is opened; it is not opened for him.");
                Kill(s, taken);
                s.Denarii = 0;
            }
            else
            {
                s.Denarii = 0;
            }
        }

        foreach (var g in s.Living.Where(x => x.Status == GladiatorStatus.Fessus && x.Vigor >= x.VigorMax - 1).ToList())
            g.Status = GladiatorStatus.Validus;

        Gladiator? volunteer = null;
        int bounty = 0;
        foreach (var line in ResolveNightOps(s, rng))
            log.Add(line);
        int nightChance = s.NightOrder == NightOrder.Rest ? 34 : 15;
        s.NightOrder = NightOrder.Rest;
        s.NightActorId = 0;
        if (rng.Next(100) < nightChance)
        {
            var (text, vol, b) = NightEvent(s, rng);
            log.Add(text);
            volunteer = vol;
            bounty = b;
        }

        if (s.DaysPlayed + 1 == 12 && s.HasHosted && s.Living.Any())
            log.Add("Twelve days, and you have edited a munus. The ludus still stands. Infamia is not erased. The barley is paid. That is a kind of victory.");

        foreach (var g in s.Familia) g.FoughtToday = false;
        s.OfferTakenToday = false;
        RefreshMarket(s, rng);
        RefreshLaborMarket(s, rng);
        RefreshOffer(s, rng);
        Calendar.Next(s);

        CheckEnd(s);

        return new EndDayResult { Log = log, Volunteer = volunteer, Bounty = bounty };
    }

    static (string text, Gladiator? volunteer, int bounty) NightEvent(GameState s, Random rng)
    {
        int n = rng.Next(8);
        switch (n)
        {
            case 0:
                if (s.Living.Any())
                {
                    var g = PickLiving(s, rng);
                    g.Status = GladiatorStatus.Aeger;
                    g.Vigor = Math.Max(1, g.Vigor - 3);
                    return ($"Fever in the cells. {g.Name} is aeger. The medicus would want coin you may not have.", null, 0);
                }
                break;
            case 1:
                if (s.Living.Count() >= 2)
                {
                    var a = PickLiving(s, rng);
                    var b = s.Living.Where(x => x.Id != a.Id).OrderBy(_ => rng.Next()).First();
                    a.Status = GladiatorStatus.Vulneratus;
                    a.Vigor = Math.Max(1, a.Vigor - 2);
                    return ($"A quarrel after the barley. {a.Name} and {b.Name}. Only {a.Name} bleeds. Juvenal would not be surprised: you quarter the light-armed too near the heavies.", null, 0);
                }
                break;
            case 2:
                if (s.Living.Count() < Beds(s))
                {
                    var taken = new HashSet<string>(s.Familia.Select(x => x.Name));
                    var g = MakeGladiator(s, rng, taken, RandomArmatura(rng), tiro: false);
                    g.Source = "auctoratus";
                    g.Virtus = Math.Min(12, g.Virtus + 2);
                    int bounty = 40 + g.Virtus * 5;
                    return ($"An auctoratus waits at the gate: {g.Name}, {Content.ArmaturaNom(g.Armatura)} {g.Origin}. He sells himself — a citizen's disgrace, a ludus's gain.", g, bounty);
                }
                break;
            case 3:
                s.Fama = Math.Min(99, s.Fama + 1);
                return ("An augustalis in Puteoli has heard the name of your ludus. Fama +1. He has not invited you to dine.", null, 0);
            case 4:
                int fine = Math.Min(s.Denarii, rng.Next(8, 22));
                s.Denarii -= fine;
                return ($"A watchman remembers that lanistae are infames. A fine of {fine} denarii for a door left unbarred. Or for existing.", null, 0);
            case 5:
                if (s.Living.Any(g => g.Fama >= 3 || g.Palmae >= 2))
                {
                    var star = s.Living.OrderByDescending(g => g.Fama + g.Palmae).First();
                    int gift = rng.Next(12, 30);
                    s.Denarii += gift;
                    star.Fama++;
                    return ($"A woman of no inscription leaves {gift} denarii at the gate for {star.Name}. The graffiti will follow.", null, 0);
                }
                return ("Night passes over the palus. Dogs. A cart on the via.", null, 0);
            case 6:
                return ("Someone chalks SPARTACUS on the outside wall. You have it washed before the watch sees. Capua has a long memory.", null, 0);
            default:
                if (s.Offer != null)
                    return ($"A boy from {s.Offer.EditorName} confirms the terms for tomorrow's light. Have a man ready.", null, 0);
                return ("Oil lamps gutter. The familia sleeps as prisoners sleep.", null, 0);
        }
        return ("The night is quiet. That too is a kind of omen.", null, 0);
    }

    public static bool AcceptAuctoratus(GameState s, Gladiator g, int bounty)
    {
        if (s.Denarii < bounty || s.Living.Count() >= Beds(s)) return false;
        s.Denarii -= bounty;
        g.Id = s.NextId++;
        s.Familia.Add(g);
        return true;
    }

    static Gladiator PickLiving(GameState s, Random rng) => s.Living.ElementAt(rng.Next(s.Living.Count()));

    public static void CheckEnd(GameState s)
    {
        bool noMen = !s.Living.Any();
        bool broke = s.Denarii <= 0;
        if (!broke && !noMen) return;
        if (broke || noMen)
        {
            s.Ended = true;
            s.EndTitle = "Ludus clausus";
            s.EndMessage = noMen && broke
                ? "The cells are empty and the purse is dead. Creditors take the palus, the rudes, the name on the gate. You are a lanista no longer. Infamia remains."
                : noMen
                    ? "The cells are empty. No tiro will come to a lanista who cannot feed the last one. Infamia remains."
                    : "The purse is empty. Creditors do not wait on tomorrow's locatio. The palus, the rudes, the name on the gate are forfeit. Infamia remains.";
        }
    }
}
