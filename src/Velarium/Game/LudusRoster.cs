using System;
using System.Collections.Generic;
using Velarium.Models;

namespace Velarium.Game
{
    public class LudusRoster
    {
        public List<Gladiator> Fighters { get; } = new();

        public static LudusRoster CreateStarterRoster()
        {
            var roster = new LudusRoster();
            roster.Fighters.AddRange(new[]
            {
                new Gladiator
                {
                    Name = "Marcus Velox",
                    Type = GladiatorType.Murmillo,
                    Strength = 72,
                    Agility = 48,
                    Endurance = 65,
                    Victories = 3
                },
                new Gladiator
                {
                    Name = "Lucius Retiarius",
                    Type = GladiatorType.Retiarius,
                    Strength = 45,
                    Agility = 78,
                    Endurance = 52,
                    Victories = 1
                },
                new Gladiator
                {
                    Name = "Cassius Thraex",
                    Type = GladiatorType.Thraex,
                    Strength = 58,
                    Agility = 62,
                    Endurance = 55,
                    Condition = GladiatorCondition.Training,
                    Victories = 0
                }
            });
            return roster;
        }

        public void PrintRoster()
        {
            Console.WriteLine("LUDUS ROSTER");
            Console.WriteLine(new string('-', 62));
            Console.WriteLine($"{"Name",-18} {"Type",-12} {"STR",4} {"AGI",4} {"END",4} {"Rating",6} {"Status",-10} {"Wins",4}");
            Console.WriteLine(new string('-', 62));

            foreach (var fighter in Fighters)
            {
                Console.WriteLine(
                    $"{fighter.Name,-18} {fighter.Type,-12} {fighter.Strength,4} {fighter.Agility,4} {fighter.Endurance,4} {fighter.CombatRating,6} {fighter.Condition,-10} {fighter.Victories,4}");
            }

            Console.WriteLine(new string('-', 62));
            Console.WriteLine($"{Fighters.Count} fighter(s) under your lanista's banner.");
            Console.WriteLine();
        }
    }
}