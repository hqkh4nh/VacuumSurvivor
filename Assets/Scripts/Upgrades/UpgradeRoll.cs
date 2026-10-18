using System;
using System.Collections.Generic;

namespace VacuumSurvivor
{
    /// Pure selection of distinct, not-yet-maxed upgrade choices for the level-up panel.
    public static class UpgradeRoll
    {
        public static List<UpgradeData> Pick(IReadOnlyList<UpgradeData> all, Func<UpgradeData, int> levelOf, int n, Random rng)
        {
            var pool = new List<UpgradeData>();
            foreach (var u in all)
                if (levelOf(u) < u.maxLevel) pool.Add(u);

            var result = new List<UpgradeData>();
            while (result.Count < n && pool.Count > 0)
            {
                int i = rng.Next(pool.Count);
                result.Add(pool[i]);
                pool.RemoveAt(i);
            }
            return result;
        }
    }
}
