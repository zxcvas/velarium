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
        s = Ludus.Start(rng, seed, pra, nom, cog);

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
        if (s.Market.Count == 0) Ludus.RefreshMarket(s, rng);
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
                case 6: EndDayScreen(); break;
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
        if (s.HostingUnlocked)
            Console.WriteLine("The duumviri will hear a petition to edit a munus of your own.");
        Ui.Rule();
    }

    string OfferLabel()
        => s.Offer != null && !s.OfferTakenToday
            ? "Locatio (rent a man to an editor)"
            : "Locatio (no editor today)";

    string HostLabel()
        => s.HostingUnlocked
            ? "Edere munus (host games — unlocked)"
            : "Edere munus (locked — fama and a palma needed)";

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
            Console.WriteLine($"Your purse: {s.Denarii} denarii. Cells free: {Math.Max(0, Ludus.CellCap - s.Living.Count())} of {Ludus.CellCap}.");
            int c = Ui.Menu("Forum", new[]
            {
                "Gladiators for sale",
                $"Medicus ({Ludus.MedicusFee} denarii a man — wounds and fever)",
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
            int price = g.Value();
            if (s.Living.Count() >= Ludus.CellCap)
            {
                Console.WriteLine("The cells are full. Eight is as many as this ludus will hold.");
                Ui.Pause();
                continue;
            }
            if (s.Denarii < price)
            {
                Console.WriteLine("The dealer laughs. Come back with coin, lanista.");
                Ui.Pause();
                continue;
            }
            if (!Ui.Confirm($"Pay {price} denarii for {g.Name}?")) continue;
            string? err = Ludus.Buy(s, c - 1);
            if (err == "full")
            {
                Console.WriteLine("The cells are full. Eight is as many as this ludus will hold.");
                Ui.Pause();
                continue;
            }
            if (err == "coin")
            {
                Console.WriteLine("The dealer laughs. Come back with coin, lanista.");
                Ui.Pause();
                continue;
            }
            if (err != null) continue;
            Console.WriteLine($"{g.Name} is led through the porta of the ludus. The familia has a new mouth to feed.");
            Autosave();
            Ui.Pause();
        }
    }

    void MedicusScreen()
    {
        var need = s.Living.Where(Ludus.NeedsMedicus).ToList();
        Ui.Clear();
        Ui.Title("Medicus");
        Ui.Wrap("He is not Galen. He is a man who has sewn more thighs than tunics. Fifteen denarii a head: wine, oil, a needle, and silence.");
        if (need.Count == 0)
        {
            Console.WriteLine("No one needs him today.");
            Ui.Pause();
            return;
        }
        int c = Ui.Menu("Treat whom?", need.Select(g => $"{g.Name} ({Content.StatusLat(g.Status)}, {g.Vigor}/{g.VigorMax}) — {Ludus.MedicusFee} den.").ToList());
        if (c == 0) return;
        var g = need[c - 1];
        if (!Ludus.Treat(s, g))
        {
            Console.WriteLine("The medicus packs his bag. Coin first.");
            Ui.Pause();
            return;
        }
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
            Ludus.DeclineOfferNoFighter(s);
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

        PlayMunus(g, hosted: false);
    }

    void HostScreen()
    {
        Ui.Clear();
        Ui.Title("Edere munus");
        if (!s.HostingUnlocked)
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

        Ui.Wrap("You petition to edit a modest munus in a wooden arena at the edge of Capua. You pay for the sand, the trumpets, a pair of officials, and a damnatus or hired foe for the other corner. Your man fights. You — not a magistrate — take the palm of the editor: mitte or iugula is yours if the other falls. The crowd will remember who gave them the show.");
        Console.WriteLine();
        Console.WriteLine($"Cost: {Ludus.HostCost} denarii. Purse: {s.Denarii}.");
        var able = s.Living.Where(g => g.CanFight).ToList();
        if (able.Count == 0)
        {
            Console.WriteLine("You have no one who can stand.");
            Ui.Pause();
            return;
        }
        if (s.Denarii < Ludus.HostCost)
        {
            Console.WriteLine("The magistrates require the money first.");
            Ui.Pause();
            return;
        }
        if (!Ui.Confirm("Stage the munus?")) return;
        Ludus.TryPayHost(s);

        int c = Ui.Menu("Which of yours takes the sand?", able.Select(g =>
            $"{g.Name}, {Content.ArmaturaNom(g.Armatura)}, virtus {g.Virtus}, {g.Rank()}").ToList());
        if (c == 0)
        {
            Ludus.RefundHost(s);
            return;
        }
        PlayMunus(able[c - 1], hosted: true);
    }

    void PlayMunus(Gladiator g, bool hosted)
    {
        var bout = Ludus.RunBout(s, rng, g, hosted);
        var foe = bout.Foe;

        Ui.Clear();
        Ui.Title(hosted ? "Munus — you are editor" : "Munus — locatio");
        if (!hosted && bout.Offer != null)
        {
            Ui.Wrap($"{bout.Offer.Venue}. Editor: {bout.Offer.EditorName}. Your {g.Name} ({Content.ArmaturaNom(g.Armatura)}) is led in from the porta sanavivaria. Opposite: {foe.Name}, {Content.ArmaturaNom(foe.Armatura)} of {bout.Offer.RivalLanista}.");
        }
        else
        {
            Ui.Wrap($"A wooden arena at Capua. Your edict promised a pair. {g.Name} ({Content.ArmaturaNom(g.Armatura)}) enters. The other corner is {foe.Name}, {Content.ArmaturaNom(foe.Armatura)}, bought cheap for the day.");
        }
        if (bout.WrongType && bout.Offer != null)
            Ui.Wrap($"The clerk frowns: they asked for a {Content.ArmaturaNom(bout.Offer.Requested)}. The crowd will know the pairing is wrong.");
        if (Content.ClassicPair(g.Armatura, foe.Armatura))
            Ui.Wrap("A proper pairing. The old men in the first seats nod.");
        Console.WriteLine();
        Ui.Wrap("Trumpets. Sand. Heat. The awning is drawn as far as the masts allow.");
        Ui.Pause("The first pass — [Enter]");

        foreach (var beat in bout.Report.Beats)
        {
            Console.WriteLine();
            Ui.Wrap(beat);
        }

        var ownFallen = IugulaChoice.SimRolls;
        var foeFallen = IugulaChoice.SimRolls;
        if (hosted)
        {
            if (bout.Report.Outcome is FightOutcome.Victoria or FightOutcome.VictoriaSineMissione)
            {
                Console.WriteLine();
                Ui.Wrap($"{foe.Name} is at your mercy. The crowd's noise is a weather. You are editor today.");
                int h = Ui.Menu("The fallen foe", new[] { "Mitte — spare him", "Iugula — have him killed" }, zeroBack: false);
                foeFallen = h == 2 ? IugulaChoice.Iugula : IugulaChoice.Mitte;
            }
            else if (bout.Report.Outcome is not FightOutcome.Stans)
            {
                Ui.Wrap("Your man is down. The crowd is a throat. You may still refuse them — he is your property.");
                int h = Ui.Menu(g.Name + " fallen", new[] { "Mitte — he is worth more alive", "Give him up (the crowd will love you, briefly)" }, zeroBack: false);
                ownFallen = h == 2 ? IugulaChoice.Iugula : IugulaChoice.Mitte;
            }
        }

        Console.WriteLine();
        Ui.Rule();
        var settled = Ludus.SettleBout(s, rng, bout, ownFallen, foeFallen);
        foreach (var line in settled.Lines)
            Ui.Wrap(line);

        Console.WriteLine();
        Console.WriteLine($"Purse: {s.Denarii} denarii. Fama ludi: {s.Fama}.");
        if (settled.HostingJustUnlocked)
        {
            Console.WriteLine();
            Ui.Wrap("A note from the duumviri: they will hear a petition to edit a munus. Infamia is not erased. It is papered over with sand.");
        }
        Autosave();
        Ui.Pause();
    }

    void EndDayScreen()
    {
        Ui.Clear();
        Ui.Title("Vespera");
        var night = Ludus.EndDay(s, rng);

        foreach (var line in night.Log)
        {
            Ui.Wrap("• " + line);
            Console.WriteLine();
        }

        if (night.Volunteer != null)
            HandleAuctoratus(night.Volunteer, night.Bounty);

        Console.WriteLine($"Tomorrow: {Calendar.Format(s)}    Purse: {s.Denarii}    Fama: {s.Fama}");
        Autosave();
        Ui.Pause();
    }

    void HandleAuctoratus(Gladiator g, int bounty)
    {
        Console.WriteLine();
        Ui.Wrap($"He asks {bounty} denarii for the oath. Volunteers fight like men who chose the sand. Purse: {s.Denarii}.");
        if (s.Denarii >= bounty && s.Living.Count() < Ludus.CellCap && Ui.Confirm("Pay the bounty and take his sacramentum?"))
        {
            if (Ludus.AcceptAuctoratus(s, g, bounty))
                Console.WriteLine($"{g.Name} is yours until the rudis, or the gate of Libitina.");
            else
                Console.WriteLine($"{g.Name} goes to another lanista. You will meet him on the sand, perhaps.");
        }
        else
        {
            Console.WriteLine($"{g.Name} goes to another lanista. You will meet him on the sand, perhaps.");
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
