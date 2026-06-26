using System;
using System.Collections.Generic;
using Velarium.Models;

namespace Velarium.Game
{
    public class LudusRoster
    {
        static readonly string[] RecruitNames =
        {
            "Gaius Fortis", "Publius Acer", "Titus Magnus", "Decimus Ferox",
            "Sextus Valens", "Aulus Primus", "Quintus Durus", "Flavius Novus"
        };

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

        public Gladiator CreateRecruit()
        {
            var type = (GladiatorType)Random.Shared.Next(Enum.GetValues<GladiatorType>().Length);
            return new Gladiator
            {
                Name = RecruitNames[Random.Shared.Next(RecruitNames.Length)],
                Type = type,
                Strength = Random.Shared.Next(40, 61),
                Agility = Random.Shared.Next(40, 61),
                Endurance = Random.Shared.Next(40, 61)
            };
        }

        public bool TryGetFighter(int oneBasedIndex, out Gladiator fighter)
        {
            if (oneBasedIndex < 1 || oneBasedIndex > Fighters.Count)
            {
                fighter = null!;
                return false;
            }

            fighter = Fighters[oneBasedIndex - 1];
            return true;
        }

        public void PrintRoster(bool showIndex = false)
        {
            Console.WriteLine("LUDUS ROSTER");
            Console.WriteLine(new string('-', showIndex ? 68 : 62));
            if (showIndex)
            {
                Console.WriteLine($"{"#",3} {"Name",-18} {"Type",-12} {"STR",4} {"AGI",4} {"END",4} {"Rating",6} {"Status",-10} {"Wins",4}");
            }
            else
            {
                Console.WriteLine($"{"Name",-18} {"Type",-12} {"STR",4} {"AGI",4} {"END",4} {"Rating",6} {"Status",-10} {"Wins",4}");
            }

            Console.WriteLine(new string('-', showIndex ? 68 : 62));

            for (var i = 0; i < Fighters.Count; i++)
            {
                var fighter = Fighters[i];
                var indexPrefix = showIndex ? $"{i + 1,3} " : "";
                Console.WriteLine(
                    $"{indexPrefix}{fighter.Name,-18} {fighter.Type,-12} {fighter.Strength,4} {fighter.Agility,4} {fighter.Endurance,4} {fighter.CombatRating,6} {fighter.Condition,-10} {fighter.Victories,4}");
            }

            Console.WriteLine(new string('-', showIndex ? 68 : 62));
            Console.WriteLine($"{Fighters.Count} fighter(s) under your lanista's banner.");
            Console.WriteLine();
        }
    }
}