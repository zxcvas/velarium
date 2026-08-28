namespace Velarium;

public enum Armatura
{
    Murmillo,
    Thraex,
    Retiarius,
    Secutor
}

public enum GladiatorStatus
{
    Validus,
    Fessus,
    Vulneratus,
    Aeger,
    Mortuus
}

public enum DayOrder
{
    None,
    Palus,
    Sparring,
    Requies
}

public enum FightOutcome
{
    Victoria,
    Stans,
    Missio,
    Mors,
    VictoriaSineMissione
}

public sealed class Gladiator
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Origin { get; set; } = "";
    public Armatura Armatura { get; set; }
    public int Vigor { get; set; }
    public int VigorMax { get; set; }
    public int Virtus { get; set; }
    public int Fama { get; set; }
    public int Palmae { get; set; }
    public int Stantes { get; set; }
    public int Missiones { get; set; }
    public int Pugnat { get; set; }
    public GladiatorStatus Status { get; set; }
    public DayOrder Order { get; set; }
    public string Source { get; set; } = "servus";
    public bool FoughtToday { get; set; }

    public bool Alive => Status != GladiatorStatus.Mortuus;
    public bool CanFight => Alive && Status != GladiatorStatus.Aeger && Vigor >= 4;

    public int Value()
    {
        int v = 180 + Virtus * 22 + Palmae * 35 + Fama * 8 + VigorMax * 4;
        if (Status == GladiatorStatus.Vulneratus) v = v * 6 / 10;
        if (Status == GladiatorStatus.Aeger) v = v * 7 / 10;
        if (Source == "auctoratus") v += 40;
        return Math.Max(80, v);
    }

    public string Rank()
    {
        if (Pugnat == 0) return "tiro";
        if (Palmae >= 8 || Virtus >= 14) return "primus palus";
        if (Pugnat >= 5) return "veteranus";
        return "gladiator";
    }

    public string Record()
        => $"pugn. {ToRoman.Small(Pugnat)}  vic. {ToRoman.Small(Palmae)}  stans {ToRoman.Small(Stantes)}  mis. {ToRoman.Small(Missiones)}";

    public string Inscription()
        => $"{Name} {Content.ArmaturaAbl(Armatura)} {Origin}, {Record()}";
}

public sealed class Contract
{
    public string EditorName { get; set; } = "";
    public string EditorOffice { get; set; } = "editor";
    public string Venue { get; set; } = "";
    public Armatura Requested { get; set; }
    public int PaySudore { get; set; }
    public int PayOccisus { get; set; }
    public string RivalLanista { get; set; } = "";
}

public sealed class GameState
{
    public int Seed { get; set; }
    public int NextId { get; set; } = 1;
    public int YearAuc { get; set; } = 782;
    public int Month { get; set; } = 5;
    public int Day { get; set; } = 1;
    public string Praenomen { get; set; } = "";
    public string Nomen { get; set; } = "";
    public string Cognomen { get; set; } = "";
    public string LudusName { get; set; } = "";
    public int Denarii { get; set; }
    public int Fama { get; set; }
    public int DaysPlayed { get; set; }
    public bool HasHosted { get; set; }
    public bool OfferTakenToday { get; set; }
    public bool Ended { get; set; }
    public string? EndTitle { get; set; }
    public string? EndMessage { get; set; }
    public List<Gladiator> Familia { get; set; } = new();
    public List<Gladiator> Market { get; set; } = new();
    public List<string> AdLibitinam { get; set; } = new();
    public Contract? Offer { get; set; }

    public string FullName => $"{Praenomen} {Nomen} {Cognomen}".Trim();
    public IEnumerable<Gladiator> Living => Familia.Where(g => g.Alive);
}

public sealed class CombatReport
{
    public FightOutcome Outcome { get; set; }
    public List<string> Beats { get; set; } = new();
    public bool Spectacular { get; set; }
    public bool PlayerDown { get; set; }
    public bool FoeDown { get; set; }
    public int PlayerVigorAfter { get; set; }
    public int CrowdBloodlust { get; set; }
}
