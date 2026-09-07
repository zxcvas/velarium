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
        if (CanUseKeys())
        {
            try { Console.ReadKey(true); }
            catch { Console.ReadLine(); }
        }
        else
            Console.ReadLine();
        Console.WriteLine();
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
        if (options.Count == 0) return 0;
        if (!CanUseKeys())
            return MenuTyped(title, options, zeroBack);

        int sel = 0;
        bool hide = true;
        try { Console.CursorVisible = false; }
        catch { hide = false; }
        Console.WriteLine();
        int top;
        try { top = Console.CursorTop; }
        catch { return MenuTyped(title, options, zeroBack); }

        while (!Eof)
        {
            try { Console.SetCursorPosition(0, top); }
            catch { return MenuTyped(title, options, zeroBack); }
            PaintMenu(title, options, sel, zeroBack);
            ConsoleKeyInfo key;
            try { key = Console.ReadKey(true); }
            catch { Eof = true; return 0; }

            if (key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar)
                return FinishMenu(hide, sel + 1);
            if (zeroBack && (key.Key is ConsoleKey.Escape or ConsoleKey.Backspace or ConsoleKey.Q
                || key.KeyChar == '0'))
                return FinishMenu(hide, 0);
            if (key.Key is ConsoleKey.UpArrow or ConsoleKey.K)
                sel = (sel - 1 + options.Count) % options.Count;
            else if (key.Key is ConsoleKey.DownArrow or ConsoleKey.Tab or ConsoleKey.J)
                sel = (sel + 1) % options.Count;
            else if (key.Key == ConsoleKey.Home)
                sel = 0;
            else if (key.Key == ConsoleKey.End)
                sel = options.Count - 1;
            else if (key.KeyChar >= '1' && key.KeyChar <= '9')
            {
                int n = key.KeyChar - '0';
                if (n <= options.Count) return FinishMenu(hide, n);
            }
        }
        return FinishMenu(hide, 0);
    }

    static int FinishMenu(bool hide, int value)
    {
        if (hide)
        {
            try { Console.CursorVisible = true; }
            catch { /* ignore */ }
        }
        Console.WriteLine();
        return value;
    }

    static void PaintMenu(string title, IReadOnlyList<string> options, int sel, bool zeroBack)
    {
        int inner = Width - 4;
        if (!string.IsNullOrEmpty(title))
            Console.WriteLine(title.PadRight(Width));
        Console.WriteLine("+" + new string('-', Width - 2) + "+");
        for (int i = 0; i < options.Count; i++)
        {
            string mark = i == sel ? ">" : " ";
            string body = $"{mark} [{i + 1}] {options[i]}";
            if (body.Length > inner) body = body[..inner];
            Console.WriteLine("| " + body.PadRight(inner) + " |");
        }
        Console.WriteLine("+" + new string('-', Width - 2) + "+");
        string hint = zeroBack
            ? "arrows/tab  Enter  1-9  Esc/0 back"
            : "arrows/tab  Enter  1-9";
        Console.WriteLine(hint.PadRight(Width));
    }

    static int MenuTyped(string title, IReadOnlyList<string> options, bool zeroBack)
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

    static bool CanUseKeys()
    {
        try
        {
            return !Console.IsInputRedirected && !Console.IsOutputRedirected;
        }
        catch
        {
            return false;
        }
    }

    public static bool Confirm(string q)
    {
        Console.Write($"{q} (s/n) > ");
        if (CanUseKeys())
        {
            try
            {
                var key = Console.ReadKey(true);
                Console.WriteLine(key.KeyChar);
                char c = char.ToLowerInvariant(key.KeyChar);
                return c is 's' or 'y';
            }
            catch { /* fall through */ }
        }
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
