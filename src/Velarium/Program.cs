namespace Velarium;

static class Program
{
    static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Amphiteater";

        if (TryReport(args)) return 0;

        string save = args.Length > 0 && args[0] == "--save" && args.Length > 1
            ? args[1]
            : Save.DefaultPath();
        new Game(save).Run();
        return 0;
    }

    static bool TryReport(string[] args)
    {
        int idx = Array.FindIndex(args, a => a == "--report");
        if (idx < 0) return false;
        int n = 200;
        if (idx + 1 < args.Length && int.TryParse(args[idx + 1], out int parsed) && parsed > 0)
            n = parsed;
        var kitchen = args.Any(a => a == "--locatio")
            ? CareerKitchenPolicy.LocatioOnly
            : CareerKitchenPolicy.UpgradeStall;
        int days = CareerSim.DefaultMaxDays;
        int daysIdx = Array.FindIndex(args, a => a == "--days");
        if (daysIdx >= 0 && daysIdx + 1 < args.Length
            && int.TryParse(args[daysIdx + 1], out int parsedDays) && parsedDays > 0)
            days = parsedDays;
        var report = CareerSim.RunMany(n, days, kitchen);
        Console.Write(CareerSim.Format(report, kitchen));
        return true;
    }
}
