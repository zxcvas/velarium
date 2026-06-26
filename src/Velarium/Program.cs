using System;
using Velarium.Game;

namespace Velarium
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintBanner();

            Console.WriteLine("VELARIUM");
            Console.WriteLine("The Awning of the Amphitheater");
            Console.WriteLine("Build 0.3.0 — Save / Load");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("A game of spectacle, strategy, and survival in ancient Rome.");
            Console.WriteLine("Manage your ludus, train fighters, and fill the amphitheater.");
            Console.WriteLine();

            var state = ResolveStartingState();

            while (true)
            {
                Console.WriteLine("MAIN MENU");
                Console.WriteLine($"Denarii: {state.Denarii}");
                Console.WriteLine("  [1] Manage Ludus");
                Console.WriteLine("  [2] Enter the Arena");
                Console.WriteLine("  [3] Read the Emperor's Edict");
                Console.WriteLine("  [4] Inspect the Velarium (about)");
                Console.WriteLine("  [5] Save Game");
                Console.WriteLine("  [6] Load Game");
                Console.WriteLine("  [7] Exit to the Via");
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
                        Console.WriteLine("Current status: Roster, economy, arena, and save/load active.");
                        Console.WriteLine();
                        break;
                    case "5":
                        GameSaveService.TrySave(state, out var saveMessage);
                        Console.WriteLine(saveMessage);
                        Console.WriteLine();
                        break;
                    case "6":
                        if (GameSaveService.TryLoad(out var loadedState, out var loadMessage) && loadedState is not null)
                        {
                            state = loadedState;
                        }
                        Console.WriteLine(loadMessage);
                        Console.WriteLine();
                        break;
                    case "7":
                    case "exit":
                    case "q":
                        Console.WriteLine("Vale, citizen of Rome. The games will continue another day.");
                        return;
                    default:
                        Console.WriteLine("The lictor does not understand your strange tongue. Try 1-7.");
                        Console.WriteLine();
                        break;
                }
            }
        }

        static GameState ResolveStartingState()
        {
            if (!GameSaveService.SaveExists())
            {
                return GameState.CreateNew();
            }

            Console.WriteLine("A saved ludus was found.");
            Console.WriteLine("  [N] Begin a new lanista career");
            Console.WriteLine("  [L] Load saved game");
            Console.Write("Choice > ");

            var input = Console.ReadLine()?.Trim().ToUpperInvariant();
            Console.WriteLine();

            if (input == "L" && GameSaveService.TryLoad(out var loadedState, out var message) && loadedState is not null)
            {
                Console.WriteLine(message);
                Console.WriteLine();
                return loadedState;
            }

            if (input == "L")
            {
                Console.WriteLine("Could not load save. Starting a new career.");
                Console.WriteLine();
            }

            return GameState.CreateNew();
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