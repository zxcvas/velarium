using System;

namespace Velarium.Game
{
    public static class GameMenus
    {
        public static void RunManageLudus(GameState state)
        {
            while (true)
            {
                Console.WriteLine("MANAGE LUDUS");
                Console.WriteLine($"Denarii: {state.Denarii}");
                Console.WriteLine($"  [1] View Roster");
                Console.WriteLine($"  [2] Recruit Fighter ({GameState.RecruitCost} denarii)");
                Console.WriteLine($"  [3] Train Fighter ({GameState.TrainCost} denarii)");
                Console.WriteLine("  [0] Back to Main Menu");
                Console.Write("Choice > ");

                var input = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        state.Roster.PrintRoster();
                        break;
                    case "2":
                        state.TryRecruit(out var recruitMessage);
                        Console.WriteLine(recruitMessage);
                        Console.WriteLine();
                        break;
                    case "3":
                        if (TryReadFighterIndex(state, "TRAIN FIGHTER", out var trainIndex))
                        {
                            state.TryTrain(trainIndex, out var trainMessage);
                            Console.WriteLine(trainMessage);
                            Console.WriteLine();
                        }
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Choose 0-3.");
                        Console.WriteLine();
                        break;
                }
            }
        }

        public static void RunArena(GameState state)
        {
            Console.WriteLine("ENTER THE ARENA");
            Console.WriteLine("Select a healthy fighter to send into the sand.");
            Console.WriteLine();

            if (TryReadFighterIndex(state, "ARENA", out var arenaIndex))
            {
                state.TryRunArena(arenaIndex, out var arenaMessage);
                Console.WriteLine(arenaMessage);
                Console.WriteLine();
            }
        }

        static bool TryReadFighterIndex(GameState state, string title, out int fighterIndex)
        {
            fighterIndex = 0;
            state.Roster.PrintRoster(showIndex: true);
            Console.WriteLine($"{title} — enter fighter number, or 0 to cancel.");
            Console.Write("Fighter # > ");

            var input = Console.ReadLine()?.Trim();
            Console.WriteLine();

            if (input == "0")
            {
                Console.WriteLine("Cancelled.");
                Console.WriteLine();
                return false;
            }

            if (!int.TryParse(input, out fighterIndex))
            {
                Console.WriteLine("Enter a valid number.");
                Console.WriteLine();
                return false;
            }

            return true;
        }
    }
}