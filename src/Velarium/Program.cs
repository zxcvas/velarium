namespace Velarium;

static class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Amphiteater";
        string save = args.Length > 0 && args[0] == "--save" && args.Length > 1
            ? args[1]
            : Save.DefaultPath();
        new Game(save).Run();
    }
}
