namespace Velarium;

public static class Calendar
{
    static readonly string[] MonthAcc =
    {
        "", "Ianuarias", "Februarias", "Martias", "Apriles", "Maias", "Iunias",
        "Iulias", "Augustas", "Septembres", "Octobres", "Novembres", "Decembres"
    };

    static readonly string[] MonthAbl =
    {
        "", "Ianuariis", "Februariis", "Martiis", "Aprilibus", "Maiis", "Iuniis",
        "Iuliis", "Augustis", "Septembribus", "Octobribus", "Novembribus", "Decembribus"
    };

    static readonly int[] Lengths = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    public static bool LongMonth(int month) => month is 3 or 5 or 7 or 10;
    public static int Nones(int month) => LongMonth(month) ? 7 : 5;
    public static int Ides(int month) => LongMonth(month) ? 15 : 13;

    public static int DaysIn(int yearAuc, int month)
    {
        if (month == 2 && IsLeap(yearAuc)) return 29;
        return Lengths[month];
    }

    // AUC years: 754 = AD 1. Julian leap years roughly: year AUC divisible? 
    // 45 BC (AUC 709) was a leap reform year. Good enough: leap if (yearAuc - 709) % 4 == 0.
    public static bool IsLeap(int yearAuc) => (yearAuc - 709) % 4 == 0;

    public static void Next(GameState s)
    {
        int dim = DaysIn(s.YearAuc, s.Month);
        s.Day++;
        if (s.Day > dim)
        {
            s.Day = 1;
            s.Month++;
            if (s.Month > 12)
            {
                s.Month = 1;
                s.YearAuc++;
            }
        }
        s.DaysPlayed++;
    }

    public static string Format(GameState s) => Format(s.YearAuc, s.Month, s.Day);

    public static string Format(int yearAuc, int month, int day)
    {
        int nones = Nones(month);
        int ides = Ides(month);
        string date;
        if (day == 1)
            date = $"Kalendis {MonthAbl[month]}";
        else if (day == nones)
            date = $"Nonis {MonthAbl[month]}";
        else if (day == ides)
            date = $"Idibus {MonthAbl[month]}";
        else if (day < nones)
            date = CountBack(nones, day, $"Nonas {MonthAcc[month]}");
        else if (day < ides)
            date = CountBack(ides, day, $"Idus {MonthAcc[month]}");
        else
        {
            int nextMonth = month == 12 ? 1 : month + 1;
            int dim = month == 2 && IsLeap(yearAuc) ? 29 : Lengths[month];
            // Inclusive count to next Kalends (day dim+1).
            date = CountBack(dim + 1, day, $"Kalendas {MonthAcc[nextMonth]}");
        }
        return $"{date}, a.u.c. {ToRoman.Convert(yearAuc)}";
    }

    static string CountBack(int target, int day, string named)
    {
        int n = target - day + 1;
        if (n == 2) return $"pridie {named}";
        return $"a.d. {ToRoman.Convert(n)} {named}";
    }
}

public static class ToRoman
{
    static readonly (int val, string sym)[] Map =
    {
        (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
        (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
        (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
    };

    public static string Convert(int n)
    {
        if (n <= 0) return "N";
        var sb = new System.Text.StringBuilder();
        foreach (var (val, sym) in Map)
        {
            while (n >= val)
            {
                sb.Append(sym);
                n -= val;
            }
        }
        return sb.ToString();
    }

    public static string Small(int n) => n <= 0 ? "—" : Convert(n);
}
