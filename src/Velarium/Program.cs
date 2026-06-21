using System;

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
            Console.WriteLine("Placeholder Build 0.0.1");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("A game of spectacle, strategy, and survival in ancient Rome.");
            Console.WriteLine("This is an initial skeleton. Real gameplay coming in future sprints.");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("MAIN MENU");
                Console.WriteLine("  [1] Enter the Arena (stub)");
                Console.WriteLine("  [2] Read the Emperor's Edict");
                Console.WriteLine("  [3] Inspect the Velarium (about)");
                Console.WriteLine("  [4] Exit to the Via");
                Console.Write("Choice > ");

                string? input = Console.ReadLine()?.Trim();
                Console.WriteLine();

                if (input == "1")
                {
                    Console.WriteLine("The gates open. Sand underfoot. The crowd is silent...");
                    Console.WriteLine("(This feature is a placeholder. Organizers have not yet dispatched the gladiators.)");
                    Console.WriteLine("You swing wildly at thin air and somehow win. +150 Denarii (imaginary)");
                    Console.WriteLine();
                }
                else if (input == "2")
                {
                    Console.WriteLine("EDICT OF THE ORGANIZER:");
                    Console.WriteLine("\"All work shall proceed through the established development cycle.");
                    Console.WriteLine("Placeholders stand until real agents are assigned and subagents execute.\"");
                    Console.WriteLine();
                }
                else if (input == "3")
                {
                    Console.WriteLine("VELARIUM");
                    Console.WriteLine("The great awning that shades the spectators from the merciless sun.");
                    Console.WriteLine("In this game you will raise, train, and pit gladiators, manage the");
                    Console.WriteLine("spectacle, navigate Roman politics, and try not to be fed to the lions.");
                    Console.WriteLine("Current status: Structural scaffolding only.");
                    Console.WriteLine();
                }
                else if (input == "4" || input?.ToLower() == "exit" || input?.ToLower() == "q")
                {
                    Console.WriteLine("Vale, citizen of Rome. The games will continue another day.");
                    break;
                }
                else
                {
                    Console.WriteLine("The lictor does not understand your strange tongue. Try 1-4.");
                    Console.WriteLine();
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
