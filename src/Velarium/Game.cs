namespace Velarium;

sealed class Game
{
    readonly string savePath;
    GameState s = null!;
    Random rng = new();

    public Game(string savePath) => this.savePath = savePath;

    public void Run()
    {
        while (true)
        {
            Ui.Clear();
            Ui.Banner();
            Console.WriteLine("AMPHITEATER");
            Console.WriteLine("A ludus in Capua. Spectacle, coin, and infamia.");
            Ui.Rule();
            var options = new List<string> { "Novum ludum (new game)" };
            if (Save.Exists(savePath)) options.Add("Continua (load)");
            else options.Add("Continua (no save)");
            options.Add("About Amphiteater");
            options.Add("Vale (exit)");
            int c = Ui.Menu("MAIN", options, zeroBack: false);
            if (Ui.Eof) return;
            if (c == 1) { if (NewGame()) Play(); }
            else if (c == 2)
            {
                if (!Load()) { Console.WriteLine("No save found on this tablet."); Ui.Pause(); }
                else Play();
            }
            else if (c == 3) About();
            else return;
        }
    }

    bool NewGame()
    {
        Ui.Clear();
        Ui.Title("Novum ludum");
        Ui.Wrap("Capua. The amphitheatre's shadow is long. Memory of Spartacus has not wholly faded from the via. You will be lanista: owner of a familia gladiatoria, vendor of sweat and, if the editor pays, of death. The curia will not have you. The crowd may.");
        Console.WriteLine();
        var labels = Content.Presets.Select(p => $"{p.Praenomen} {p.Nomen} {p.Cognomen}").ToList();
        labels.Add("Compose a Roman name");
        int c = Ui.Menu("Who are you?", labels, zeroBack: true);
        if (c == 0) return false;

        string pra, nom, cog, bio;
        if (c <= Content.Presets.Length)
        {
            var p = Content.Presets[c - 1];
            pra = p.Praenomen; nom = p.Nomen; cog = p.Cognomen; bio = p.Bio;
        }
        else
        {
            pra = PickOrType("Praenomen", Content.Praenomina);
            nom = PickOrType("Nomen gentile", Content.Nomina);
            cog = PickOrType("Cognomen", Content.Cognomina);
            bio = "A new lanista in Capua, known only to the auctioneers and the watch.";
        }

        int seed = Environment.TickCount;
        rng = new Random(seed);
        s = new GameState
        {
            Seed = seed,
            YearAuc = 782,
            Month = 5,
            Day = 1,
            Praenomen = pra,
            Nomen = nom,
            Cognomen = cog,
            LudusName = "Ludus " + nom,
            Denarii = 620,
            Fama = 3,
            NextId = 1
        };

        var taken = new HashSet<string>();
        Armatura[] kit = { Armatura.Murmillo, Armatura.Thraex, Armatura.Retiarius };
        foreach (var a in kit)
            s.Familia.Add(MakeGladiator(taken, a, tiro: true));

        RefreshMarket();
        RefreshOffer();

        Ui.Clear();
        Ui.Title(s.LudusName);
        Console.WriteLine(s.FullName + ", lanista");
        Console.WriteLine();
        Ui.Wrap(bio);
        Console.WriteLine();
        Ui.Wrap("You begin on the Kalends of May, a.u.c. DCCLXXXII, with three tiros, a handful of denarii, and a contract-law older than your father: twenty for sweat, a fortune if they die. Do not spend your men cheaply. Do not keep them idle. The barley still wants paying.");
        Ui.Pause();
        Autosave();
        return true;
    }

    string PickOrType(string label, string[] list)
    {
        int c = Ui.Menu(label, list.Concat(new[] { "Type it" }).ToList());
        if (c == 0) return list[0];
        if (c == list.Length + 1)
        {
            string t = Ui.Read(label + " > ");
            return string.IsNullOrWhiteSpace(t) ? list[0] : t.Trim();
        }
        return list[c - 1];
    }

    bool Load()
    {
        var loaded = Save.Read(savePath);
        if (loaded == null) return false;
        s = loaded;
        rng = new Random(unchecked(s.Seed + s.DaysPlayed * 997));
        if (s.Market.Count == 0) RefreshMarket();
        return true;
    }

    void Autosave()
    {
        try { Save.Write(savePath, s); }
        catch (Exception ex) { Console.WriteLine("(Could not save: " + ex.Message + ")"); }
    }

    void Play()
    {
        while (!s.Ended && !Ui.Eof)
        {
            Ui.Clear();
            PrintStatus();
            var items = new List<string>
            {
                "Familia gladiatoria",
                "Exercitia (orders for the yard)",
                "Forum (buy, medicus, rumors)",
                OfferLabel(),
                HostLabel(),
                "Finis diei (end the day)",
                "Servare et abire (save and leave)"
            };
            int c = Ui.Menu(null!, items, zeroBack: false);
            switch (c)
            {
                case 1: FamiliaScreen(); break;
                case 2: OrdersScreen(); break;
                case 3: ForumScreen(); break;
                case 4: LocatioScreen(); break;
                case 5: HostScreen(); break;
                case 6: EndDay(); break;
                case 7: Autosave(); return;
            }
            if (s.Ended) ShowEnding();
        }
    }

    void PrintStatus()
    {
        Ui.Title(s.LudusName + " — Capua");
        Ui.Header(s.FullName + ", lanista", Calendar.Format(s));
        int living = s.Living.Count();
        Ui.Header($"Denarii: {s.Denarii}", $"Fama ludi: {s.Fama}    Familia: {living}");
        if (s.Offer != null && !s.OfferTakenToday)
            Console.WriteLine($"Today's locatio: {s.Offer.EditorName} wants a {Content.ArmaturaNom(s.Offer.Requested)}.");
        else if (s.OfferTakenToday)
            Console.WriteLine("The munus for today is done.");
        else
            Console.WriteLine("No editor came to the ludus this morning.");
        if (HostingUnlocked())
            Console.WriteLine("The duumviri will hear a petition to edit a munus of your own.");
        Ui.Rule();
    }

    string OfferLabel()
        => s.Offer != null && !s.OfferTakenToday
            ? "Locatio (rent a man to an editor)"
            : "Locatio (no editor today)";

    string HostLabel()
        => HostingUnlocked()
            ? "Edere munus (host games — unlocked)"
            : "Edere munus (locked — fama and a palma needed)";

    bool HostingUnlocked()
        => s.Fama >= 16 && s.Living.Any(g => g.Palmae >= 1);

    void FamiliaScreen()
    {
        while (true)
        {
            Ui.Clear();
            Ui.Title("Familia of " + s.LudusName);
            var living = s.Living.ToList();
            if (living.Count == 0)
            {
                Ui.Wrap("The cells are empty. The palus stands idle. Buy flesh at the forum or close the ludus.");
            }
            else
            {
                int primusId = living.OrderByDescending(g => g.Virtus).ThenByDescending(g => g.Palmae).First().Id;
                Console.WriteLine($"{"#",2} {"NAME",-14} {"ARM",-10} {"ORIG",-10} {"VIG",5} {"VIR",3} {"STATUS",-12} RANK");
                Ui.Rule();
                for (int i = 0; i < living.Count; i++)
                {
                    var g = living[i];
                    string rank = g.Id == primusId && g.Pugnat > 0 ? "primus palus" : g.Rank();
                    Console.WriteLine($"{i + 1,2} {g.Name,-14} {Content.ArmaturaNom(g.Armatura),-10} {g.Origin,-10} {g.Vigor,2}/{g.VigorMax,-2} {g.Virtus,3} {Content.StatusLat(g.Status),-12} {rank}");
                    Console.WriteLine($"    {g.Record()}    {Content.OrderLat(g.Order)}    {g.Source}");
                }
            }
            if (s.AdLibitinam.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Ad Libitinam (the dead):");
                foreach (var d in s.AdLibitinam.TakeLast(8))
                    Console.WriteLine("  " + d);
            }
            Console.WriteLine();
            int c = Ui.Menu("Inspect a man, or return.", living.Select(g => g.Name + " — " + Content.ArmaturaNom(g.Armatura)).ToList());
            if (c == 0) return;
            Inspect(living[c - 1]);
        }
    }

    void Inspect(Gladiator g)
    {
        Ui.Clear();
        Ui.Title(g.Name);
        Console.WriteLine($"{Content.ArmaturaNom(g.Armatura).ToUpperInvariant()}  |  natus {g.Origin}  |  {g.Source}");
        Console.WriteLine(g.Record());
        Console.WriteLine($"Vigor {g.Vigor}/{g.VigorMax}    Virtus {g.Virtus}    Fama {g.Fama}    Value {g.Value()} denarii");
        Console.WriteLine("Arms: " + Content.ArmaturaKit(g.Armatura));
        Console.WriteLine("Status: " + Content.StatusLat(g.Status) + "    Order: " + Content.OrderLat(g.Order));
        Console.WriteLine();
        Ui.Wrap(g.Pugnat == 0
            ? "A tiro. He has not yet seen the harena. The palus knows him; the crowd does not."
            : $"{g.Name} has gone down onto the sand {g.Pugnat} time(s). The familia measures him by palmae, not by years.");
        Ui.Pause();
    }

    void OrdersScreen()
    {
        while (true)
        {
            var living = s.Living.ToList();
            Ui.Clear();
            Ui.Title("Exercitia");
            Ui.Wrap("Dawn drills. Novices at the palus (the wooden stake). Veterans with rudes (wooden swords). The wounded to requies. Orders are resolved when the day ends — unless the man is sent to the munus, in which case the yard can wait.");
            Console.WriteLine();
            if (living.Count == 0) { Ui.Pause(); return; }
            int c = Ui.Menu("Whose order?", living.Select(g => $"{g.Name} ({Content.StatusLat(g.Status)}) now: {Content.OrderLat(g.Order)}").ToList());
            if (c == 0) return;
            var g = living[c - 1];
            int o = Ui.Menu(g.Name, new[] { "Ad palum (safe drill, small gain)", "Sparring with rudes (faster, risk of injury)", "Requies (rest and the medicus's cheap wine)" });
            if (o == 0) continue;
            g.Order = o switch { 1 => DayOrder.Palus, 2 => DayOrder.Sparring, _ => DayOrder.Requies };
            Console.WriteLine($"{g.Name}: {Content.OrderLat(g.Order)}.");
            Ui.Pause();
        }
    }

    void ForumScreen()
    {
        while (true)
        {
            Ui.Clear();
            Ui.Title("Forum of Capua");
            Ui.Wrap("Slave-dealers in the shade of the portico. Oil, barley, a medicus who has worked the ludi before. Somewhere an aedile's clerk is nailing an edictum muneris to a wall — a velarium promised, if the wind allows.");
            Console.WriteLine();
            Console.WriteLine($"Your purse: {s.Denarii} denarii. Cells free: {Math.Max(0, 8 - s.Living.Count())} of 8.");
            int c = Ui.Menu("Forum", new[]
            {
                "Gladiators for sale",
                "Medicus (15 denarii a man — wounds and fever)",
                "Rumors"
            });
            if (c == 0) return;
            if (c == 1) MarketScreen();
            else if (c == 2) MedicusScreen();
            else Rumors();
        }
    }

    void MarketScreen()
    {
        while (true)
        {
            Ui.Clear();
            Ui.Title("In catasta (on the sale-platform)");
            if (s.Market.Count == 0)
            {
                Ui.Wrap("The dealers have nothing left today that would not shame a ludus. Return tomorrow.");
                Ui.Pause();
                return;
            }
            var labels = s.Market.Select(g =>
                $"{g.Name}, {Content.ArmaturaNom(g.Armatura)} {g.Origin}, virtus {g.Virtus}, {g.Value()} den. ({g.Source})").ToList();
            int c = Ui.Menu("Buy whom?", labels);
            if (c == 0) return;
            var g = s.Market[c - 1];
            if (s.Living.Count() >= 8)
            {
                Console.WriteLine("The cells are full. Eight is as many as this ludus will hold.");
                Ui.Pause();
                continue;
            }
            int price = g.Value();
            if (s.Denarii < price)
            {
                Console.WriteLine("The dealer laughs. Come back with coin, lanista.");
                Ui.Pause();
                continue;
            }
            if (!Ui.Confirm($"Pay {price} denarii for {g.Name}?")) continue;
            s.Denarii -= price;
            g.Id = s.NextId++;
            g.Order = DayOrder.None;
            s.Familia.Add(g);
            s.Market.RemoveAt(c - 1);
            Console.WriteLine($"{g.Name} is led through the porta of the ludus. The familia has a new mouth to feed.");
            Autosave();
            Ui.Pause();
        }
    }

    void MedicusScreen()
    {
        var need = s.Living.Where(g => g.Status is GladiatorStatus.Vulneratus or GladiatorStatus.Aeger || g.Vigor < g.VigorMax).ToList();
        Ui.Clear();
        Ui.Title("Medicus");
        Ui.Wrap("He is not Galen. He is a man who has sewn more thighs than tunics. Fifteen denarii a head: wine, oil, a needle, and silence.");
        if (need.Count == 0)
        {
            Console.WriteLine("No one needs him today.");
            Ui.Pause();
            return;
        }
        int c = Ui.Menu("Treat whom?", need.Select(g => $"{g.Name} ({Content.StatusLat(g.Status)}, {g.Vigor}/{g.VigorMax}) — 15 den.").ToList());
        if (c == 0) return;
        var g = need[c - 1];
        if (s.Denarii < 15)
        {
            Console.WriteLine("The medicus packs his bag. Coin first.");
            Ui.Pause();
            return;
        }
        s.Denarii -= 15;
        g.Vigor = Math.Min(g.VigorMax, g.Vigor + 6);
        if (g.Status is GladiatorStatus.Vulneratus or GladiatorStatus.Aeger)
            g.Status = g.Vigor >= g.VigorMax - 2 ? GladiatorStatus.Validus : GladiatorStatus.Fessus;
        Console.WriteLine($"{g.Name} is bound and dosed. He will live to cost you more barley.");
        Autosave();
        Ui.Pause();
    }

    void Rumors()
    {
        Ui.Clear();
        Ui.Title("Rumores");
        string[] rum = {
            "On the wall: an edictum muneris. The aedile promises pairs, a hunt at noon, and a velarium if the sun bites.",
            "Celadus the thraex — not yours — is still the suspirium puellarum in Pompeii. Graffiti has a long memory.",
            "A clerk cites Gaius: twenty denarii pro sudore if they walk out whole; a thousand if occisi or debilitati. The law knows what you sell.",
            "The ludus Iulianus at Capua still casts a shadow. Do not let your men talk of Spartacus in the yard.",
            "A rival lanista at Puteoli is buying retiarii. The pairing with secutores fills seats.",
            "Barley is dear this month. The hordearii will eat what you give them, and hate you either way.",
            "Augustalis games at Puteoli next market-cycle. Editors will come as far as Capua looking for a murmillo who can stand."
        };
        Ui.Wrap(rum[rng.Next(rum.Length)]);
        if (s.Offer != null && !s.OfferTakenToday)
        {
            Console.WriteLine();
            Ui.Wrap($"You already know today's editor: {s.Offer.EditorName}, {s.Offer.EditorOffice}, seeking a {Content.ArmaturaNom(s.Offer.Requested)} for {s.Offer.Venue}.");
        }
        Ui.Pause();
    }

    void LocatioScreen()
    {
        Ui.Clear();
        Ui.Title("Locatio");
        if (s.OfferTakenToday)
        {
            Ui.Wrap("The afternoon munus is over. One lease a day; the editors do not wait on a second familia.");
            Ui.Pause();
            return;
        }
        if (s.Offer == null)
        {
            Ui.Wrap("No editor sent a boy to the gate. Train. Rest. Tomorrow the forum may bring a clerk with a tablet.");
            Ui.Pause();
            return;
        }

        var o = s.Offer;
        Ui.Wrap($"{o.EditorName}, {o.EditorOffice}, seeks a {Content.ArmaturaNom(o.Requested)} for {o.Venue}.");
        Console.WriteLine();
        Ui.Wrap($"Terms, in the manner of the jurists: {o.PaySudore} denarii pro sudore if he leaves the harena whole; {o.PayOccisus} if occisus or broken. You are not the editor. You rent the man. The crowd will shout mitte or iugula; the editor decides.");
        Console.WriteLine();
        Ui.Wrap($"The other corner: a man of {o.RivalLanista}.");

        var able = s.Living.Where(g => g.CanFight).ToList();
        if (able.Count == 0)
        {
            Console.WriteLine();
            Ui.Wrap("No one in the familia can stand. The editor's clerk makes a mark against your name.");
            s.Fama = Math.Max(0, s.Fama - 1);
            s.OfferTakenToday = true;
            Ui.Pause();
            return;
        }

        int c = Ui.Menu("Send whom?", able.Select(g =>
        {
            string mark = g.Armatura == o.Requested ? "requested type" : "wrong type";
            string risk = g.Status == GladiatorStatus.Validus ? "" : " [" + Content.StatusLat(g.Status) + "]";
            return $"{g.Name}, {Content.ArmaturaNom(g.Armatura)}, virtus {g.Virtus}, {g.Rank()} ({mark}){risk}";
        }).ToList());
        if (c == 0) return;
        var g = able[c - 1];
        if (g.Status != GladiatorStatus.Validus && !Ui.Confirm($"{g.Name} is {Content.StatusLat(g.Status)}. Send him anyway?"))
            return;

        ResolveMunus(g, hosted: false);
    }

    void HostScreen()
    {
        Ui.Clear();
        Ui.Title("Edere munus");
        if (!HostingUnlocked())
        {
            Ui.Wrap("The duumviri will not grant you the staging of a munus. A lanista is infamis; an editor is a public man. Raise the fama of the ludus (need 16) and return with a man who has taken a palma. Then they may pretend not to see what you are.");
            Console.WriteLine();
            Console.WriteLine($"Fama ludi now: {s.Fama}. Palma in house: {(s.Living.Any(g => g.Palmae >= 1) ? "yes" : "not yet")}.");
            Ui.Pause();
            return;
        }
        if (s.OfferTakenToday)
        {
            Ui.Wrap("You have already stained the sand today. The magistrates allotted one afternoon.");
            Ui.Pause();
            return;
        }

        const int cost = 220;
        Ui.Wrap("You petition to edit a modest munus in a wooden arena at the edge of Capua. You pay for the sand, the trumpets, a pair of officials, and a damnatus or hired foe for the other corner. Your man fights. You — not a magistrate — take the palm of the editor: mitte or iugula is yours if the other falls. The crowd will remember who gave them the show.");
        Console.WriteLine();
        Console.WriteLine($"Cost: {cost} denarii. Purse: {s.Denarii}.");
        var able = s.Living.Where(g => g.CanFight).ToList();
        if (able.Count == 0)
        {
            Console.WriteLine("You have no one who can stand.");
            Ui.Pause();
            return;
        }
        if (s.Denarii < cost)
        {
            Console.WriteLine("The magistrates require the money first.");
            Ui.Pause();
            return;
        }
        if (!Ui.Confirm("Stage the munus?")) return;
        s.Denarii -= cost;

        int c = Ui.Menu("Which of yours takes the sand?", able.Select(g =>
            $"{g.Name}, {Content.ArmaturaNom(g.Armatura)}, virtus {g.Virtus}, {g.Rank()}").ToList());
        if (c == 0)
        {
            s.Denarii += cost;
            return;
        }
        ResolveMunus(able[c - 1], hosted: true);
        s.HasHosted = true;
    }

    void ResolveMunus(Gladiator g, bool hosted)
    {
        Armatura foeType = hosted
            ? Content.ClassicFoe(g.Armatura)
            : (rng.Next(100) < 80 ? s.Offer!.Requested : Content.ClassicFoe(g.Armatura));
        // If renting, opponent is the complement of the requested pairing.
        if (!hosted && s.Offer != null)
            foeType = Content.ClassicFoe(s.Offer.Requested);

        var foe = Combat.MakeFoe(rng, foeType, s.DaysPlayed);
        if (!hosted && s.Offer != null && rng.Next(100) < 70)
            foe.Armatura = Content.ClassicFoe(s.Offer.Requested);

        Ui.Clear();
        Ui.Title(hosted ? "Munus — you are editor" : "Munus — locatio");
        if (!hosted && s.Offer != null)
        {
            Ui.Wrap($"{s.Offer.Venue}. Editor: {s.Offer.EditorName}. Your {g.Name} ({Content.ArmaturaNom(g.Armatura)}) is led in from the porta sanavivaria. Opposite: {foe.Name}, {Content.ArmaturaNom(foe.Armatura)} of {s.Offer.RivalLanista}.");
        }
        else
        {
            Ui.Wrap($"A wooden arena at Capua. Your edict promised a pair. {g.Name} ({Content.ArmaturaNom(g.Armatura)}) enters. The other corner is {foe.Name}, {Content.ArmaturaNom(foe.Armatura)}, bought cheap for the day.");
        }
        if (!hosted && s.Offer != null && g.Armatura != s.Offer.Requested)
            Ui.Wrap($"The clerk frowns: they asked for a {Content.ArmaturaNom(s.Offer.Requested)}. The crowd will know the pairing is wrong.");
        if (Content.ClassicPair(g.Armatura, foe.Armatura))
            Ui.Wrap("A proper pairing. The old men in the first seats nod.");
        Console.WriteLine();
        Ui.Wrap("Trumpets. Sand. Heat. The awning is drawn as far as the masts allow.");
        Ui.Pause("The first pass — [Enter]");

        var report = Combat.Fight(rng, g, foe);
        foreach (var beat in report.Beats)
        {
            Console.WriteLine();
            Ui.Wrap(beat);
        }

        g.Pugnat++;
        g.FoughtToday = true;
        g.Vigor = Math.Max(1, report.PlayerVigorAfter);
        if (g.Vigor < g.VigorMax / 2) g.Status = GladiatorStatus.Fessus;

        bool wrongType = !hosted && s.Offer != null && g.Armatura != s.Offer.Requested;
        int pay = 0;
        int famaDelta = 0;

        Console.WriteLine();
        Ui.Rule();

        if (report.Outcome == FightOutcome.Stans)
        {
            g.Stantes++;
            g.Virtus = Math.Min(18, g.Virtus + (rng.Next(2) == 0 ? 1 : 0));
            g.Fama++;
            pay = hosted ? rng.Next(90, 140) : Sudore(wrongType);
            famaDelta = report.Spectacular ? 2 : 1;
            Ui.Wrap($"Stans. Both leave the sand. The crowd is divided; the clerks are not. Pro sudore: {pay} denarii.");
        }
        else if (report.Outcome == FightOutcome.Victoria || report.Outcome == FightOutcome.VictoriaSineMissione)
        {
            bool killFoe = false;
            if (hosted)
            {
                Console.WriteLine();
                Ui.Wrap($"{foe.Name} is at your mercy. The crowd's noise is a weather. You are editor today.");
                int h = Ui.Menu("The fallen foe", new[] { "Mitte — spare him", "Iugula — have him killed" }, zeroBack: false);
                killFoe = h == 2;
            }
            else
            {
                killFoe = report.CrowdBloodlust >= 3 && rng.Next(100) < 40;
                Ui.Wrap(killFoe
                    ? $"The crowd wants the sword. The editor does not lift his hand. {foe.Name} is finished."
                    : $"Shouts of mitte. The editor waves the wooden staff. {foe.Name} lives to be rented again.");
            }

            g.Palmae++;
            g.Virtus = Math.Min(18, g.Virtus + 1);
            g.Fama += killFoe ? 2 : 1;
            g.Vigor = Math.Max(g.Vigor, 3);
            pay = hosted
                ? rng.Next(160, 280) + (killFoe ? 40 : 0) + (report.Spectacular ? 30 : 0)
                : Sudore(wrongType) + 25 + g.Palmae * 2;
            famaDelta = (killFoe ? 2 : 1) + (report.Spectacular ? 1 : 0) + (hosted ? 3 : 0);
            if (wrongType) famaDelta = Math.Max(0, famaDelta - 1);
            Ui.Wrap($"{g.Name} takes the palma. {(hosted ? "Gate and gifts" : "The editor's purse")}: {pay} denarii.");
        }
        else
        {
            // player down or yielded — missio or mors
            bool iugula;
            if (hosted)
            {
                Ui.Wrap("Your man is down. The crowd is a throat. You may still refuse them — he is your property.");
                int h = Ui.Menu(g.Name + " fallen", new[] { "Mitte — he is worth more alive", "Give him up (the crowd will love you, briefly)" }, zeroBack: false);
                iugula = h == 2;
            }
            else
            {
                int chance = 25 + report.CrowdBloodlust * 10 - g.Fama * 4 - g.Palmae * 3;
                if (report.Spectacular) chance -= 10;
                iugula = rng.Next(100) < Math.Clamp(chance, 8, 70);
                Ui.Wrap(iugula
                    ? "Iugula. The editor's hand turns. A man with a blade walks out from the porta libitinensis."
                    : "Mitte. The crowd has a use for him yet. He is dragged toward the gate of the living.");
            }

            if (iugula)
            {
                pay = hosted ? rng.Next(40, 90) : Occisus(wrongType);
                famaDelta = hosted ? 1 : 0;
                Ui.Wrap($"{g.Name} dies on the sand. {(hosted ? "The crowd has its death. Your purse has a hole." : $"Compensation for a destroyed man: {pay} denarii. You have sold more than sweat.")}");
                Kill(g);
            }
            else
            {
                g.Missiones++;
                g.Status = GladiatorStatus.Vulneratus;
                g.Vigor = Math.Max(1, g.VigorMax / 4);
                g.Virtus = Math.Min(18, g.Virtus + (rng.Next(3) == 0 ? 1 : 0));
                pay = hosted ? rng.Next(70, 120) : Sudore(wrongType) * 2 / 3;
                famaDelta = report.Spectacular ? 1 : 0;
                Ui.Wrap($"Missio. {g.Name} will eat barley on his back for a while. Pay: {pay} denarii.");
            }
        }

        s.Denarii += pay;
        s.Fama = Math.Clamp(s.Fama + famaDelta, 0, 99);
        s.OfferTakenToday = true;
        if (!hosted) s.Offer = null;

        Console.WriteLine();
        Console.WriteLine($"Purse: {s.Denarii} denarii. Fama ludi: {s.Fama}.");
        if (!s.HasHosted && HostingUnlocked())
        {
            Console.WriteLine();
            Ui.Wrap("A note from the duumviri: they will hear a petition to edit a munus. Infamia is not erased. It is papered over with sand.");
        }
        Autosave();
        Ui.Pause();
    }

    int Sudore(bool wrong) => Math.Max(12, (s.Offer?.PaySudore ?? 28) - (wrong ? 8 : 0));
    int Occisus(bool wrong) => Math.Max(80, (s.Offer?.PayOccisus ?? 420) - (wrong ? 40 : 0));

    void Kill(Gladiator g)
    {
        g.Status = GladiatorStatus.Mortuus;
        g.Vigor = 0;
        s.AdLibitinam.Add($"{g.Name}, {Content.ArmaturaNom(g.Armatura)} {g.Origin}, {g.Record()}");
    }

    void EndDay()
    {
        Ui.Clear();
        Ui.Title("Vespera");
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
                    g.Vigor = Math.Min(g.VigorMax, g.Vigor + 4);
                    if (g.Status is GladiatorStatus.Fessus or GladiatorStatus.Vulneratus or GladiatorStatus.Aeger)
                    {
                        if (rng.Next(100) < 55) g.Status = GladiatorStatus.Validus;
                    }
                    log.Add($"{g.Name} to requies. Barley, oil, sleep.");
                    break;
                default:
                    g.Vigor = Math.Min(g.VigorMax, g.Vigor + 1);
                    break;
            }
            g.Order = DayOrder.None;
        }

        int mouths = s.Living.Count();
        int upkeep = 10 + mouths * 6;
        s.Denarii -= upkeep;
        log.Add($"Cibaria and the ludus: -{upkeep} denarii ({mouths} mouths + roof).");

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
                Kill(taken);
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
        if (rng.Next(100) < 34)
        {
            var (text, vol, b) = NightEvent();
            log.Add(text);
            volunteer = vol;
            bounty = b;
        }

        if (s.DaysPlayed + 1 == 12 && s.HasHosted && s.Living.Any())
            log.Add("Twelve days, and you have edited a munus. The ludus still stands. Infamia is not erased. The barley is paid. That is a kind of victory.");

        foreach (var g in s.Familia) g.FoughtToday = false;
        s.OfferTakenToday = false;
        RefreshMarket();
        RefreshOffer();
        Calendar.Next(s);

        foreach (var line in log)
        {
            Ui.Wrap("• " + line);
            Console.WriteLine();
        }

        if (volunteer != null)
            HandleAuctoratus(volunteer, bounty);

        Console.WriteLine($"Tomorrow: {Calendar.Format(s)}    Purse: {s.Denarii}    Fama: {s.Fama}");
        CheckEnd();
        Autosave();
        Ui.Pause();
    }

    (string text, Gladiator? volunteer, int bounty) NightEvent()
    {
        int n = rng.Next(8);
        switch (n)
        {
            case 0:
                if (s.Living.Any())
                {
                    var g = PickLiving();
                    g.Status = GladiatorStatus.Aeger;
                    g.Vigor = Math.Max(1, g.Vigor - 3);
                    return ($"Fever in the cells. {g.Name} is aeger. The medicus would want coin you may not have.", null, 0);
                }
                break;
            case 1:
                if (s.Living.Count() >= 2)
                {
                    var a = PickLiving();
                    var b = s.Living.Where(x => x.Id != a.Id).OrderBy(_ => rng.Next()).First();
                    a.Status = GladiatorStatus.Vulneratus;
                    a.Vigor = Math.Max(1, a.Vigor - 2);
                    return ($"A quarrel after the barley. {a.Name} and {b.Name}. Only {a.Name} bleeds. Juvenal would not be surprised: you quarter the light-armed too near the heavies.", null, 0);
                }
                break;
            case 2:
                if (s.Living.Count() < 8)
                {
                    var taken = new HashSet<string>(s.Familia.Select(x => x.Name));
                    var g = MakeGladiator(taken, RandomArmatura(), tiro: false);
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

    void HandleAuctoratus(Gladiator g, int bounty)
    {
        Console.WriteLine();
        Ui.Wrap($"He asks {bounty} denarii for the oath. Volunteers fight like men who chose the sand. Purse: {s.Denarii}.");
        if (s.Denarii >= bounty && s.Living.Count() < 8 && Ui.Confirm("Pay the bounty and take his sacramentum?"))
        {
            s.Denarii -= bounty;
            g.Id = s.NextId++;
            s.Familia.Add(g);
            Console.WriteLine($"{g.Name} is yours until the rudis, or the gate of Libitina.");
        }
        else
        {
            Console.WriteLine($"{g.Name} goes to another lanista. You will meet him on the sand, perhaps.");
        }
    }

    Gladiator PickLiving() => s.Living.ElementAt(rng.Next(s.Living.Count()));

    void CheckEnd()
    {
        bool noMen = !s.Living.Any();
        bool broke = s.Denarii <= 0;
        int cheapest = s.Market.Count > 0 ? s.Market.Min(g => g.Value()) : 9999;
        if (noMen && (broke || s.Denarii < cheapest))
        {
            s.Ended = true;
            s.EndTitle = "Ludus clausus";
            s.EndMessage = "The cells are empty and the purse is dead. Creditors take the palus, the rudes, the name on the gate. You are a lanista no longer. Infamia remains.";
        }
    }

    void ShowEnding()
    {
        Ui.Clear();
        Ui.Title(s.EndTitle ?? "Finis");
        Ui.Wrap(s.EndMessage ?? "The games end.");
        Console.WriteLine();
        Console.WriteLine($"Days: {s.DaysPlayed}    Fama: {s.Fama}    Denarii: {s.Denarii}");
        if (s.AdLibitinam.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Ad Libitinam:");
            foreach (var d in s.AdLibitinam)
                Console.WriteLine("  " + d);
        }
        try { File.Delete(savePath); } catch { /* keep going */ }
        Ui.Pause();
    }

    void RefreshMarket()
    {
        s.Market.Clear();
        int n = rng.Next(1, 3);
        var taken = new HashSet<string>(s.Familia.Select(g => g.Name));
        for (int i = 0; i < n; i++)
            s.Market.Add(MakeGladiator(taken, RandomArmatura(), tiro: rng.Next(100) < 60));
    }

    void RefreshOffer()
    {
        // First two mornings always bring an editor so the core loop is visible.
        if (s.DaysPlayed >= 2 && rng.Next(100) < 32)
        {
            s.Offer = null;
            return;
        }
        var req = RandomArmatura();
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
            RivalLanista = Content.RivalLanistae[rng.Next(Content.RivalLanistae.Length)]
        };
    }

    Armatura RandomArmatura() => (Armatura)rng.Next(4);

    Gladiator MakeGladiator(HashSet<string> taken, Armatura a, bool tiro)
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

    void About()
    {
        Ui.Clear();
        Ui.Title("Amphiteater");
        Ui.Wrap("Amphiteater: a ludus in Capua, under the awning of the games. You are lanista. The velarium is only shade.");
        Console.WriteLine();
        Ui.Wrap("You are a lanista in Capua in the early Principate: infamis, a vendor of men, barred from honour and not from profit. You do not begin as editor of games. You train a familia, lease them (locatio) to magistrates and rich men, and take coin pro sudore — for sweat — or a much larger sum if they are killed. That is the contract the jurist Gaius remembered.");
        Console.WriteLine();
        Ui.Wrap("If the ludus earns fama, and a man of yours takes a palma, the duumviri may let you stage a munus of your own. Then you are editor for an afternoon: mitte or iugula is your hand.");
        Console.WriteLine();
        Console.WriteLine("Pairings the crowd expects:");
        Console.WriteLine("  murmillo  vs  thraex");
        Console.WriteLine("  retiarius vs  secutor");
        Console.WriteLine();
        Console.WriteLine("Terms: tiro (novice), palma (win), missio (reprieve), stans (draw),");
        Console.WriteLine("       rudis (wooden sword / later, discharge), harena (sand).");
        Console.WriteLine("Currency: denarii. Date: Roman civil calendar, a.u.c.");
        Ui.Pause();
    }
}
