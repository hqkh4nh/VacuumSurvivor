using System;
using System.Collections.Generic;
using System.Linq;

namespace VacuumSurvivor
{
    /// One row of the high-score board. Serializable so SaveSystem can write it as JSON.
    [Serializable]
    public struct HighScoreEntry
    {
        public int score;
        public int seconds;
        public int level;
        public string date;
    }

    /// The top-5 board. Pure, so it is unit-testable in EditMode; persistence lives in SaveSystem.
    public static class HighScoreTable
    {
        public const int Capacity = 5;

        /// Returns a new board with the entry folded in, sorted by score descending and trimmed to
        /// five. A tie keeps the already-present entry ahead of the newcomer.
        public static List<HighScoreEntry> Insert(List<HighScoreEntry> table, HighScoreEntry entry)
        {
            var combined = new List<HighScoreEntry>(table) { entry };
            // OrderByDescending is a stable sort, and the new entry is last, so on a tie the
            // existing rows stay ahead of it.
            return combined.OrderByDescending(e => e.score).Take(Capacity).ToList();
        }
    }
}
