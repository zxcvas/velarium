using System;
using Velarium.Models;

namespace Velarium.Game
{
    public class GameState
    {
        public const int RecruitCost = 250;
        public const int TrainCost = 75;

        public LudusRoster Roster { get; }
        public int Denarii { get; set; }

        public GameState(LudusRoster roster, int startingDenarii)
        {
            Roster = roster;
            Denarii = startingDenarii;
        }

        public static GameState CreateNew() => new(LudusRoster.CreateStarterRoster(), 500);

        public bool TryRecruit(out string message)
        {
            if (Denarii < RecruitCost)
            {
                message = $"Not enough denarii. Recruiting costs {RecruitCost}; you have {Denarii}.";
                return false;
            }

            var recruit = Roster.CreateRecruit();
            Roster.Fighters.Add(recruit);
            Denarii -= RecruitCost;
            message = $"Recruited {recruit.Name} ({recruit.Type}) for {RecruitCost} denarii. Balance: {Denarii}.";
            return true;
        }

        public bool TryTrain(int fighterIndex, out string message)
        {
            if (!Roster.TryGetFighter(fighterIndex, out var fighter))
            {
                message = "No fighter matches that number.";
                return false;
            }

            if (fighter.Condition is GladiatorCondition.Injured or GladiatorCondition.Dead)
            {
                message = $"{fighter.Name} cannot train while {fighter.Condition.ToString().ToLower()}.";
                return false;
            }

            if (Denarii < TrainCost)
            {
                message = $"Not enough denarii. Training costs {TrainCost}; you have {Denarii}.";
                return false;
            }

            var stat = Random.Shared.Next(3);
            var gain = Random.Shared.Next(3, 7);
            var statName = stat switch
            {
                0 => TrainStat(fighter, f => f.Strength, (f, v) => f.Strength = v, "Strength", gain),
                1 => TrainStat(fighter, f => f.Agility, (f, v) => f.Agility = v, "Agility", gain),
                _ => TrainStat(fighter, f => f.Endurance, (f, v) => f.Endurance = v, "Endurance", gain)
            };

            fighter.Condition = GladiatorCondition.Training;
            Denarii -= TrainCost;
            message = $"{fighter.Name} trained {statName}. Cost: {TrainCost} denarii. Balance: {Denarii}.";
            return true;
        }

        public bool TryRunArena(int fighterIndex, out string message)
        {
            if (!Roster.TryGetFighter(fighterIndex, out var fighter))
            {
                message = "No fighter matches that number.";
                return false;
            }

            if (fighter.Condition != GladiatorCondition.Healthy)
            {
                message = $"{fighter.Name} is {fighter.Condition.ToString().ToLower()} and cannot enter the arena.";
                return false;
            }

            var payout = fighter.CombatRating * 2 + Random.Shared.Next(20, 51);
            fighter.Victories++;
            Denarii += payout;
            message =
                $"{fighter.Name} enters the sand. The crowd roars!\n" +
                $"Victory! The lanista earns {payout} denarii. {fighter.Name} now has {fighter.Victories} win(s).\n" +
                $"Balance: {Denarii}.";
            return true;
        }

        static string TrainStat(
            Gladiator fighter,
            Func<Gladiator, int> getter,
            Action<Gladiator, int> setter,
            string label,
            int gain)
        {
            setter(fighter, Math.Min(99, getter(fighter) + gain));
            return label;
        }
    }
}