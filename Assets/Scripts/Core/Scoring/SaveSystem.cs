using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Reads and writes the high-score board as JSON under the platform's persistent data path.
    /// A missing or unreadable file is treated as an empty board rather than an error.
    public static class SaveSystem
    {
        [System.Serializable]
        class Board { public List<HighScoreEntry> entries = new List<HighScoreEntry>(); }

        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "highscores.json");

        public static List<HighScoreEntry> Load()
        {
            try
            {
                if (!File.Exists(Path)) return new List<HighScoreEntry>();
                var board = JsonUtility.FromJson<Board>(File.ReadAllText(Path));
                return board?.entries ?? new List<HighScoreEntry>();
            }
            catch
            {
                return new List<HighScoreEntry>();
            }
        }

        public static void Save(List<HighScoreEntry> entries)
        {
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(new Board { entries = entries }));
            }
            catch (IOException e)
            {
                Debug.LogWarning("Could not save high scores: " + e.Message);
            }
        }
    }
}
