using System.Collections.Generic;

namespace Velarium.Game
{
    public class SaveData
    {
        public int Version { get; set; } = 1;
        public int Denarii { get; set; }
        public List<GladiatorSaveData> Fighters { get; set; } = new();
    }

    public class GladiatorSaveData
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Endurance { get; set; }
        public string Condition { get; set; } = "";
        public int Victories { get; set; }
    }
}