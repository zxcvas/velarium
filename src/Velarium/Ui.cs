namespace Velarium;

static class Ui
{
    public const int Width = 78;

    public static void Clear()
    {
        try { Console.Clear(); } catch { /* redirected output */ }
    }

    public static void Rule(char c = '-')
        => Console.WriteLine(new string(c, Width));

    public static void Title(string text)
    {
        Console.WriteLine();
        Console.WriteLine(text.ToUpperInvariant());
        Rule('=');
    }

    public static void Header(string left, string right = "")
    {
        if (string.IsNullOrEmpty(right))
        {
            Console.WriteLine(left);
            return;
        }
        int gap = Width - left.Length - right.Length;
        if (gap < 1) gap = 1;
        Console.WriteLine(left + new string(' ', gap) + right);
    }

    public static void Wrap(string text, string indent = "")
    {
        int max = Width - indent.Length;
        foreach (string paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (paragraph.Length == 0)
            {
                Console.WriteLine();
                continue;
            }
            string[] words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var line = new System.Text.StringBuilder(indent);
            foreach (string w in words)
            {
                int extra = line.Length == indent.Length ? 0 : 1;
                if (line.Length + extra + w.Length > indent.Length + max && line.Length > indent.Length)
                {
                    Console.WriteLine(line);
                    line.Clear();
                    line.Append(indent);
                    line.Append(w);
                }
                else
                {
                    if (line.Length > indent.Length) line.Append(' ');
                    line.Append(w);
                }
            }
            Console.WriteLine(line);
        }
    }

    public static void Pause(string msg = "[Enter]")
    {
        Console.WriteLine();
        Console.Write(msg + " ");
        Console.ReadLine();
    }

    public static bool Eof { get; private set; }

    public static string Read(string prompt)
    {
        Console.Write(prompt);
        string? line = Console.ReadLine();
        if (line == null)
        {
            Eof = true;
            return "";
        }
        return line.Trim();
    }

    public static int Menu(string title, IReadOnlyList<string> options, bool zeroBack = true)
    {
        if (!string.IsNullOrEmpty(title))
        {
            Console.WriteLine();
            Console.WriteLine(title);
        }
        for (int i = 0; i < options.Count; i++)
            Console.WriteLine($"  [{i + 1}] {options[i]}");
        if (zeroBack)
            Console.WriteLine("  [0] Return");
        Console.WriteLine();
        while (!Eof)
        {
            string raw = Read("Choice > ");
            if (Eof) return 0;
            if (int.TryParse(raw, out int n))
            {
                if (zeroBack && n == 0) return 0;
                if (n >= 1 && n <= options.Count) return n;
            }
            Console.WriteLine("The lictor waits. Choose a listed number.");
        }
        return 0;
    }

    public static bool Confirm(string q)
    {
        Console.Write($"{q} (s/n) > ");
        string r = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        return r is "s" or "y" or "yes" or "sic" or "ita";
    }

    public static void Banner()
    {
        Console.WriteLine(@"
   _   __  __  ___  _  _  ___  _____  ___    _   _____  ___   ___
  /_\ |  \/  || _ \| || ||_ _||_   _|| __|  /_\ |_   _|| __| | _ \
 / _ \| |\/| ||  _/| __ | | |  | |  | _|  / _ \  | |  | _|  |   /
/_/ \_\_|  |_||_|  |_||_||___| |_|  |___|/_/ \_\ |_|  |___| |_|_\
");
    }
}
