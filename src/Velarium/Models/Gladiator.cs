namespace Velarium.Models
{
    public class Gladiator
    {
        public string Name { get; init; } = "";
        public GladiatorType Type { get; init; }
        public int Strength { get; init; }
        public int Agility { get; init; }
        public int Endurance { get; init; }
        public GladiatorCondition Condition { get; set; } = GladiatorCondition.Healthy;
        public int Victories { get; set; }

        public int CombatRating => (Strength + Agility + Endurance) / 3;
    }
}