namespace Velarium;

public static partial class Ludus
{
    public static IEnumerable<Worker> LivingWorkers(GameState s)
        => s.Household.Where(w => w.Alive);

    public static Room? RoomOf(GameState s, RoomKind kind)
        => s.Rooms.FirstOrDefault(r => r.Kind == kind);

    public static bool Staffed(GameState s, RoomKind kind)
    {
        var room = RoomOf(s, kind);
        if (room is not { Built: true, AssignedWorkerId: > 0 }) return false;
        var w = s.Household.FirstOrDefault(x => x.Id == room.AssignedWorkerId);
        return w is { CanWork: true };
    }

    public static int Beds(GameState s)
    {
        int lvl = RoomOf(s, RoomKind.Cellae)?.Level ?? 1;
        return Math.Max(CellCap, 6 + lvl * 2);
    }

    public static int TreatFee(GameState s)
        => Staffed(s, RoomKind.Medicus) ? Math.Max(8, MedicusFee - 3 - RoomOf(s, RoomKind.Medicus)!.Level) : MedicusFee;

    public static int KitchenBonus(GameState s)
        => Staffed(s, RoomKind.Kitchen) ? RoomOf(s, RoomKind.Kitchen)!.Level : 0;

    public static int Upkeep(GameState s)
    {
        int mouths = s.Living.Count();
        int per = UpkeepPerMouth;
        if (Staffed(s, RoomKind.Kitchen))
            per = Math.Max(4, UpkeepPerMouth - RoomOf(s, RoomKind.Kitchen)!.Level);
        int hands = LivingWorkers(s).Count();
        return UpkeepRoof + mouths * per + hands * WorkerUpkeep;
    }

    public static void EnsureHouse(GameState s, Random rng)
    {
        s.Rooms ??= new();
        s.Household ??= new();
        s.LaborMarket ??= new();
        if (s.Rooms.Count == 0)
        {
            s.Rooms.Add(new Room { Kind = RoomKind.Palus, Level = 1 });
            s.Rooms.Add(new Room { Kind = RoomKind.Cellae, Level = 1 });
            s.Rooms.Add(new Room { Kind = RoomKind.Kitchen, Level = 1 });
            s.Rooms.Add(new Room { Kind = RoomKind.Porta, Level = 1 });
            s.Rooms.Add(new Room { Kind = RoomKind.Medicus, Level = 0 });
        }
        s.Rival ??= MakeRival(rng);
        if (s.LaborMarket.Count == 0)
            RefreshLaborMarket(s, rng);
    }

    public static RivalLudus MakeRival(Random rng)
        => new()
        {
            Name = Content.RivalLanistae[rng.Next(Content.RivalLanistae.Length)],
            City = Content.RivalCities[rng.Next(Content.RivalCities.Length)],
            Fama = rng.Next(6, 12),
            Hostility = 0
        };

    public static Worker MakeWorker(GameState s, Random rng, HashSet<string> taken)
    {
        string name = Content.UniqueHouseholdName(rng, taken);
        taken.Add(name);
        var w = new Worker
        {
            Id = s.NextId++,
            Name = name,
            Origin = Content.Origins[rng.Next(Content.Origins.Length)],
            VigorMax = rng.Next(8, 13)
        };
        w.Vigor = w.VigorMax;
        return w;
    }

    public static void RefreshLaborMarket(GameState s, Random rng)
    {
        s.LaborMarket.Clear();
        int n = rng.Next(0, 3);
        var taken = new HashSet<string>(s.Household.Select(w => w.Name));
        for (int i = 0; i < n; i++)
            s.LaborMarket.Add(MakeWorker(s, rng, taken));
    }

    public static string? BuyWorker(GameState s, int marketIndex)
    {
        if (marketIndex < 0 || marketIndex >= s.LaborMarket.Count) return "gone";
        if (LivingWorkers(s).Count() >= HouseholdCap) return "full";
        var w = s.LaborMarket[marketIndex];
        int price = w.Value();
        if (s.Denarii < price) return "coin";
        s.Denarii -= price;
        w.Id = s.NextId++;
        s.Household.Add(w);
        s.LaborMarket.RemoveAt(marketIndex);
        return null;
    }

    public static Worker? WorkerOn(GameState s, Room room)
        => room.AssignedWorkerId <= 0 ? null : s.Household.FirstOrDefault(w => w.Id == room.AssignedWorkerId);

    public static void Unassign(GameState s, int workerId)
    {
        foreach (var r in s.Rooms.Where(r => r.AssignedWorkerId == workerId))
            r.AssignedWorkerId = 0;
    }

    public static string? AssignWorker(GameState s, int workerId, RoomKind kind)
    {
        var w = s.Household.FirstOrDefault(x => x.Id == workerId);
        if (w is not { CanWork: true }) return "idle";
        var room = RoomOf(s, kind);
        if (room == null) return "gone";
        if (kind is RoomKind.Palus or RoomKind.Cellae) return "noneed";
        if (!room.Built && kind != RoomKind.Medicus) return "unbuilt";
        if (!room.Built && kind == RoomKind.Medicus && !room.PendingUpgrade) return "unbuilt";
        Unassign(s, workerId);
        if (room.AssignedWorkerId != 0 && room.AssignedWorkerId != workerId)
            Unassign(s, room.AssignedWorkerId);
        room.AssignedWorkerId = workerId;
        return null;
    }

    public static int UpgradeCost(Room room)
    {
        if (room.Level >= MaxRoomLevel) return 0;
        return room.Kind switch
        {
            RoomKind.Medicus when room.Level == 0 => 90,
            RoomKind.Cellae => 80 + room.Level * 40,
            RoomKind.Kitchen => 70 + room.Level * 20,
            RoomKind.Porta => 60 + room.Level * 20,
            RoomKind.Palus => 50 + room.Level * 25,
            _ => 80 + room.Level * 30
        };
    }

    public static bool NeedsLaborToUpgrade(Room room)
        => room.Kind is not (RoomKind.Palus or RoomKind.Cellae);

    public static string? BeginUpgrade(GameState s, RoomKind kind)
    {
        var room = RoomOf(s, kind);
        if (room == null) return "gone";
        if (room.Level >= MaxRoomLevel) return "max";
        if (room.PendingUpgrade) return "pending";
        if (NeedsLaborToUpgrade(room))
        {
            bool labor = Staffed(s, kind)
                || (kind == RoomKind.Medicus && room.Level == 0 && LivingWorkers(s).Any(w => w.CanWork));
            if (!labor) return "labor";
        }
        int cost = UpgradeCost(room);
        if (s.Denarii < cost) return "coin";
        s.Denarii -= cost;
        room.PendingUpgrade = true;
        return null;
    }

    public static void CompleteUpgrades(GameState s, List<string> log)
    {
        foreach (var room in s.Rooms.Where(r => r.PendingUpgrade).ToList())
        {
            bool laborOk = !NeedsLaborToUpgrade(room) || Staffed(s, room.Kind)
                || (room.Kind == RoomKind.Medicus && room.Level == 0 && LivingWorkers(s).Any(w => w.CanWork));
            if (!laborOk)
            {
                log.Add($"{Content.RoomNom(room.Kind)}: the work stalls. No one was assigned.");
                continue;
            }
            room.Level++;
            room.PendingUpgrade = false;
            log.Add($"{Content.RoomNom(room.Kind)} is raised to level {room.Level}.");
        }
    }

    public static void RecoverDetained(GameState s, Random rng, List<string> log)
    {
        foreach (var w in LivingWorkers(s).Where(w => w.Detained).ToList())
        {
            if (rng.Next(100) < 55)
            {
                w.Detained = false;
                log.Add($"{w.Name} is returned to the porta, bruised and silent.");
            }
        }
    }

    public static List<string> ResolveNightOps(GameState s, Random rng)
    {
        var log = new List<string>();
        if (s.NightOrder is NightOrder.Rest or 0)
            return log;
        if (s.Rival == null)
            s.Rival = MakeRival(rng);

        string actorName;
        bool worker = s.NightActorIsWorker;
        Worker? w = null;
        Gladiator? g = null;
        if (worker)
        {
            w = s.Household.FirstOrDefault(x => x.Id == s.NightActorId && x.CanWork);
            if (w == null)
                w = LivingWorkers(s).FirstOrDefault(x => x.CanWork);
            if (w == null)
            {
                log.Add("No household slave can go out tonight. The order dies in the yard.");
                return log;
            }
            actorName = w.Name;
        }
        else
        {
            g = s.Living.FirstOrDefault(x => x.Id == s.NightActorId && x.CanFight);
            if (g == null)
                g = s.Living.FirstOrDefault(x => x.CanFight);
            if (g == null)
            {
                log.Add("No gladiator can leave the cells tonight.");
                return log;
            }
            actorName = g.Name;
        }

        int catchChance = s.NightOrder switch
        {
            NightOrder.Spy => 22,
            NightOrder.Poison => 32,
            NightOrder.Sabotage => 38,
            _ => 20
        };
        catchChance += s.Rival.Hostility * 3;
        if (Staffed(s, RoomKind.Porta)) catchChance -= 10 + RoomOf(s, RoomKind.Porta)!.Level * 2;
        if (!worker) catchChance -= 12;
        catchChance = Math.Clamp(catchChance, 8, 70);

        bool caught = rng.Next(100) < catchChance;
        int successNeed = s.NightOrder switch
        {
            NightOrder.Spy => 28,
            NightOrder.Poison => 42,
            _ => 48
        };
        if (!worker) successNeed -= 10;
        bool success = !caught && rng.Next(100) >= successNeed;

        if (caught)
        {
            s.Rival.Hostility = Math.Min(12, s.Rival.Hostility + 2);
            log.Add($"{actorName} is taken at the gate of {s.Rival.Name} in {s.Rival.City}.");
            if (worker && w != null)
            {
                if (rng.Next(100) < 45)
                {
                    w.Alive = false;
                    Unassign(s, w.Id);
                    log.Add($"They do not send {w.Name} back. The household is one mouth lighter.");
                }
                else
                {
                    w.Detained = true;
                    Unassign(s, w.Id);
                    log.Add($"A fine and a bruise. {w.Name} is held. Hostility grows.");
                    int fine = Math.Min(s.Denarii, rng.Next(12, 28));
                    s.Denarii -= fine;
                    if (fine > 0) log.Add($"The watch extracts {fine} denarii.");
                }
            }
            else if (g != null)
            {
                if (rng.Next(100) < 25)
                {
                    Kill(s, g);
                    log.Add($"{g.Name} does not come back from {s.Rival.City}.");
                }
                else
                {
                    g.Status = GladiatorStatus.Vulneratus;
                    g.Vigor = Math.Max(1, g.VigorMax / 3);
                    log.Add($"{g.Name} is beaten home. He will not stand tomorrow.");
                }
            }
            return log;
        }

        if (!success)
        {
            log.Add($"{actorName} finds nothing useful at {s.Rival.Name}'s ludus. Dogs, a barred porta.");
            return log;
        }

        switch (s.NightOrder)
        {
            case NightOrder.Spy:
                s.Rival.Intel = $"{s.Rival.Name} in {s.Rival.City}: fama {s.Rival.Fama}, hostility {s.Rival.Hostility}. A star murmillo is rumored; they hire often.";
                log.Add($"{actorName} listens at the rival's porta. {s.Rival.Intel}");
                break;
            case NightOrder.Poison:
                s.Rival.NextFoePoisoned = true;
                log.Add($"Wine is left at the gate of {s.Rival.Name}. Tomorrow their man may enter the sand already sick. (Gameplay concession.)");
                break;
            case NightOrder.Sabotage:
                s.Rival.MissTomorrow = true;
                s.Rival.Fama = Math.Max(0, s.Rival.Fama - 1);
                log.Add($"{actorName} fouls a latch and a water jar at {s.Rival.Name}'s ludus. They may miss tomorrow's editor.");
                break;
        }

        return log;
    }
}
