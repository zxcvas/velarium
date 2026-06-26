using System;
using Velarium.Game;

namespace Velarium
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var state = GameState.CreateNew();

            PrintBanner();

            Console.WriteLine("VELARIUM");
            Console.WriteLine("The Awning of the Amphitheater");
            Console.WriteLine("Build 0.2.0 — Ludus Management");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("A game of spectacle, strategy, and survival in ancient Rome.");
            Console.WriteLine("Manage your ludus, train fighters, and fill the amphitheater.");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("MAIN MENU");
                Console.WriteLine($"Denarii: {state.Denarii}");
                Console.WriteLine("  [1] Manage Ludus");
                Console.WriteLine("  [2] Enter the Arena");
                Console.WriteLine("  [3] Read the Emperor's Edict");
                Console.WriteLine("  [4] Inspect the Velarium (about)");
                Console.WriteLine("  [5] Exit to the Via");
                Console.Write("Choice > ");

                string? input = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        GameMenus.RunManageLudus(state);
                        break;
                    case "2":
                        GameMenus.RunArena(state);
                        break;
                    case "3":
                        Console.WriteLine("EDICT OF THE ORGANIZER:");
                        Console.WriteLine("\"All work shall proceed through the established development cycle.");
                        Console.WriteLine("Small, reviewable steps. Keep the exe runnable after every change.\"");
                        Console.WriteLine();
                        break;
                    case "4":
                        Console.WriteLine("VELARIUM");
                        Console.WriteLine("The great awning that shades the spectators from the merciless sun.");
                        Console.WriteLine("In this game you will raise, train, and pit gladiators, manage the");
                        Console.WriteLine("spectacle, navigate Roman politics, and try not to be fed to the lions.");
                        Console.WriteLine("Current status: Roster, economy, recruit/train, and arena stubs active.");
                        Console.WriteLine();
                        break;
                    case "5":
                    case "exit":
                    case "q":
                        Console.WriteLine("Vale, citizen of Rome. The games will continue another day.");
                        return;
                    default:
                        Console.WriteLine("The lictor does not understand your strange tongue. Try 1-5.");
                        Console.WriteLine();
                        break;
                }
            }
        }

        static void PrintBanner()
        {
            string banner = @"
 __   __  _______  ___      _______  ______    ___   __   __  __   __ 
|  | |  ||       ||   |    |   _   ||    _ |  |   | |  | |  ||  | |  |
|  |_|  ||    ___||   |    |  |_|  ||   | ||  |   | |  | |  ||  | |  |
|       ||   |___ |   |    |       ||   |_||_ |   | |  |_|  ||  |_|  |
|       ||    ___||   |___ |       ||    __  ||   | |       ||       |
 |     | |   |___ |       ||   _   ||   |  | ||   | |       ||       |
  |___|  |_______||_______||__| |__||___|  |_||___| |_______||_______|
";
            Console.WriteLine(banner);
        }
    }
}