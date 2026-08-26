namespace Velarium;

static class Content
{
    public static readonly string[] GladiatorNames =
    {
        "Flamma", "Spiculus", "Celadus", "Columbus", "Tetraites", "Priscus", "Verus",
        "Triumphus", "Pugnax", "Ferox", "Hilarus", "Oceanus", "Astyanax", "Rutuba",
        "Nobilior", "Purpurio", "Scylax", "Bato", "Entellus", "Creticus", "Bebryx",
        "Hermes", "Pinna", "Asiaticus", "Maximus", "Urbicus", "Maternus", "Severus",
        "Victor", "Audax", "Celer", "Fortis", "Beryllus", "Amarantus", "Nomas",
        "Licentiosus", "Barosus", "Cubicularius", "Rapidus", "Diodorus", "Meleager",
        "Pardus", "Tigris", "Capreolus", "Smaragdus", "Niger", "Albus", "Eros",
        "Nicephorus", "Stratocles", "Galeotes", "Crescens", "Spiculus", "Hippolytus"
    };

    public static readonly string[] Origins =
    {
        "Thrax", "Gallus", "Germanus", "Afer", "Syrus", "Graecus",
        "Hispanus", "Dacus", "Cappadox", "Samnis", "Aegyptius", "Delmata"
    };

    public static readonly string[] Praenomina =
    {
        "Gaius", "Lucius", "Marcus", "Quintus", "Publius", "Titus",
        "Aulus", "Sextus", "Decimus", "Gnaeus"
    };

    public static readonly string[] Nomina =
    {
        "Atinius", "Velius", "Hostilius", "Naevius", "Gabinius", "Sempronius",
        "Oppius", "Furius", "Varius", "Calpurnius", "Domitius", "Cassius",
        "Salvius", "Vibius", "Helvius"
    };

    public static readonly string[] Cognomina =
    {
        "Strabo", "Capito", "Felix", "Corvus", "Naso", "Celer", "Rufus",
        "Severus", "Bassus", "Varro", "Fronto", "Cursor", "Merula", "Piso"
    };

    public static readonly string[] EditorNames =
    {
        "Aulus Clodius Flaccus",
        "Numerius Popidius Ampliatus",
        "Marcus Holconius Rufus",
        "Gaius Cuspius Pansa",
        "Lucius Valerius Primus",
        "Publius Vedius Nummianus",
        "Quintus Spedius Firmus",
        "Titus Suedius Clemens",
        "Gaius Quinctius Valgus",
        "Marcus Epidius Hymenaeus",
        "Lucius Ceius Secundus",
        "Aulus Umbricius Scaurus"
    };

    public static readonly string[] EditorOffices =
    {
        "aedile", "duumvir", "editor", "augustalis", "wealthy freedman"
    };

    public static readonly string[] Venues =
    {
        "The amphitheatre of Capua",
        "The games at Puteoli",
        "A munus at Nola",
        "The stone amphitheatre at Pompeii",
        "A wooden arena at Atella",
        "The games at Neapolis",
        "A funeral munus outside the Porta Stabiana"
    };

    public static readonly string[] RivalLanistae =
    {
        "Gnaeus Salvius", "Quintus Fabius Cilo", "the familia of Naevius",
        "Marcus Tettienus", "a lanista out of Puteoli", "the ludus of the Julii"
    };

    public sealed record LanistaPreset(string Praenomen, string Nomen, string Cognomen, string Bio);

    public static readonly LanistaPreset[] Presets =
    {
        new("Lucius", "Atinius", "Strabo",
            "Son of a freedman who bought a decaying ludus after the old master died of fever. The cells still smell of barley and oil. Capua remembers Spartacus; the watchmen remember him louder."),
        new("Gaius", "Velius", "Capito",
            "A veteran of the Rhine. He spent his donative on two slaves and a lease near the amphitheatre. The centurionate is closed to him now; the harena is not."),
        new("Marcus", "Hostilius", "Felix",
            "A Campanian hungry for honour he can never hold. Infamia bars the curia. Coin and blood may yet buy a kind of name."),
        new("Quintus", "Naevius", "Corvus",
            "He bought two tiros at a dawn auction and little else. The creditors in Neapolis know his gait. Capua is far enough — for a season."),
        new("Publius", "Gabinius", "Naso",
            "Once a doctor (trainer) in a larger ludus. He knows the palus, the rudis, and the cost of a cheap medicus. Now the familia is his."),
        new("Aulus", "Sempronius", "Celer",
            "Quick-handed and quicker-tongued. He talks like a duumvir and pays like a lanista. The aediles already dislike him, which is a kind of fame.")
    };

    public static string ArmaturaNom(Armatura a) => a switch
    {
        Armatura.Murmillo => "murmillo",
        Armatura.Thraex => "thraex",
        Armatura.Retiarius => "retiarius",
        Armatura.Secutor => "secutor",
        _ => a.ToString().ToLowerInvariant()
    };

    public static string ArmaturaAbl(Armatura a) => a switch
    {
        Armatura.Murmillo => "murmillo",
        Armatura.Thraex => "thraex",
        Armatura.Retiarius => "retiarius",
        Armatura.Secutor => "secutor",
        _ => ArmaturaNom(a)
    };

    public static string ArmaturaKit(Armatura a) => a switch
    {
        Armatura.Murmillo => "galea with a fish-crest, scutum, gladius, manica",
        Armatura.Thraex => "griffin-crest helm, parmula, sica, long greaves",
        Armatura.Retiarius => "no helm, galerus on the shoulder, rete and fuscina",
        Armatura.Secutor => "smooth helm, tiny eye-holes, scutum, gladius — made to hunt the net",
        _ => ""
    };

    public static bool ClassicPair(Armatura a, Armatura b)
        => (a == Armatura.Murmillo && b == Armatura.Thraex)
        || (a == Armatura.Thraex && b == Armatura.Murmillo)
        || (a == Armatura.Retiarius && b == Armatura.Secutor)
        || (a == Armatura.Secutor && b == Armatura.Retiarius);

    public static Armatura ClassicFoe(Armatura a) => a switch
    {
        Armatura.Murmillo => Armatura.Thraex,
        Armatura.Thraex => Armatura.Murmillo,
        Armatura.Retiarius => Armatura.Secutor,
        Armatura.Secutor => Armatura.Retiarius,
        _ => Armatura.Thraex
    };

    public static string StatusLat(GladiatorStatus s) => s switch
    {
        GladiatorStatus.Validus => "validus",
        GladiatorStatus.Fessus => "fessus",
        GladiatorStatus.Vulneratus => "vulneratus",
        GladiatorStatus.Aeger => "aeger",
        GladiatorStatus.Mortuus => "mortuus",
        _ => s.ToString().ToLowerInvariant()
    };

    public static string OrderLat(DayOrder o) => o switch
    {
        DayOrder.Palus => "ad palum",
        DayOrder.Sparring => "sparring with rudes",
        DayOrder.Requies => "requies",
        _ => "idle"
    };

    public static string Beat(Random rng, Armatura a, bool advantage)
    {
        string[] murmilloAdv =
        {
            "The murmillo comes on behind the scutum, short gladius seeking the gap at the neck.",
            "Fish-crest dipping, he bullies with the shield-boss and stamps for room.",
            "He plants his feet like a legionary and waits for the lighter man to err."
        };
        string[] murmilloDef =
        {
            "The heavy shield drops a finger too low; the crest turns, blind for a breath.",
            "The murmillo's scutum is a wall, but a wall that tires.",
            "Sand cakes the visor. He shakes his head like an ox."
        };
        string[] thraexAdv =
        {
            "The thraex slips off the line; the sica hooks around the rim of the larger shield.",
            "Griffin-helm flashes. He cuts at the sword-arm, dancing on the long greaves.",
            "Parmula high, he feints low and the curved blade bites."
        };
        string[] thraexDef =
        {
            "The small shield is all he has; a heavy blow drives him back toward the podium.",
            "The sica scrapes bronze and finds no flesh.",
            "He is quicker — until the sand drinks his wind."
        };
        string[] retiAdv =
        {
            "The rete flies, wet with spray, seeking helm and shoulder.",
            "Bare-headed, the retiarius circles; the fuscina darts like a trident of Neptune.",
            "He gives ground, always the net hand ready, galerus turned to take a cut."
        };
        string[] retiDef =
        {
            "The net falls short and is a burden. He tugs, cursed, as the pursuer closes.",
            "No helm, no shield: a cut along the ribs opens red.",
            "The trident rings on the scutum and numbs his fingers."
        };
        string[] secAdv =
        {
            "The secutor comes on without pause — the pursuer, as he was made.",
            "Smooth helm, two dark eye-holes: the net finds no purchase on that bronze skull.",
            "He pins the net-man toward the wall, gladius short and honest."
        };
        string[] secDef =
        {
            "The tiny eye-holes cost him. He turns too late; the trident kisses the greave.",
            "Heat builds inside the smooth helm. He staggers, hunting a shape.",
            "A cast of the net, almost — he hacks the ropes instead of the man."
        };

        string[] pool = (a, advantage) switch
        {
            (Armatura.Murmillo, true) => murmilloAdv,
            (Armatura.Murmillo, false) => murmilloDef,
            (Armatura.Thraex, true) => thraexAdv,
            (Armatura.Thraex, false) => thraexDef,
            (Armatura.Retiarius, true) => retiAdv,
            (Armatura.Retiarius, false) => retiDef,
            (Armatura.Secutor, true) => secAdv,
            _ => secDef
        };
        return pool[rng.Next(pool.Length)];
    }

    public static string Pick(Random rng, params string[] lines) => lines[rng.Next(lines.Length)];

    public static string UniqueName(Random rng, HashSet<string> taken)
    {
        for (int i = 0; i < 40; i++)
        {
            string n = GladiatorNames[rng.Next(GladiatorNames.Length)].ToUpperInvariant();
            if (!taken.Contains(n)) return n;
        }
        return (GladiatorNames[rng.Next(GladiatorNames.Length)] + " " + Origins[rng.Next(Origins.Length)]).ToUpperInvariant();
    }
}
