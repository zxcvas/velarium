using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Velarium.Models;

namespace Velarium.Game
{
    public static class GameSaveService
    {
        public const string DefaultSavePath = "saves/velarium_save.json";

        static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public static bool SaveExists(string path = DefaultSavePath) => File.Exists(path);

        public static bool TrySave(GameState state, out string message, string path = DefaultSavePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var saveData = ToSaveData(state);
                var json = JsonSerializer.Serialize(saveData, JsonOptions);
                File.WriteAllText(path, json);
                message = $"Game saved to {path}.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Save failed: {ex.Message}";
                return false;
            }
        }

        public static bool TryLoad(out GameState? state, out string message, string path = DefaultSavePath)
        {
            state = null;

            if (!File.Exists(path))
            {
                message = $"No save file found at {path}.";
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                var saveData = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
                if (saveData is null)
                {
                    message = "Save file is empty or unreadable.";
                    return false;
                }

                state = FromSaveData(saveData);
                message = $"Game loaded from {path}. Denarii: {state.Denarii}, fighters: {state.Roster.Fighters.Count}.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Load failed: {ex.Message}";
                return false;
            }
        }

        static SaveData ToSaveData(GameState state)
        {
            return new SaveData
            {
                Version = 1,
                Denarii = state.Denarii,
                Fighters = state.Roster.Fighters.Select(fighter => new GladiatorSaveData
                {
                    Name = fighter.Name,
                    Type = fighter.Type.ToString(),
                    Strength = fighter.Strength,
                    Agility = fighter.Agility,
                    Endurance = fighter.Endurance,
                    Condition = fighter.Condition.ToString(),
                    Victories = fighter.Victories
                }).ToList()
            };
        }

        static GameState FromSaveData(SaveData saveData)
        {
            var roster = new LudusRoster();
            roster.Fighters.AddRange(saveData.Fighters.Select(ToGladiator));
            return new GameState(roster, saveData.Denarii);
        }

        static Gladiator ToGladiator(GladiatorSaveData data)
        {
            if (!Enum.TryParse<GladiatorType>(data.Type, out var type))
            {
                type = GladiatorType.Murmillo;
            }

            if (!Enum.TryParse<GladiatorCondition>(data.Condition, out var condition))
            {
                condition = GladiatorCondition.Healthy;
            }

            return new Gladiator
            {
                Name = data.Name,
                Type = type,
                Strength = data.Strength,
                Agility = data.Agility,
                Endurance = data.Endurance,
                Condition = condition,
                Victories = data.Victories
            };
        }
    }
}