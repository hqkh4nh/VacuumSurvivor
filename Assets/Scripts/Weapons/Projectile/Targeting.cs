using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Pure nearest-point selection, isolated from scene state so it is unit-testable.
    public static class Targeting
    {
        public static int Nearest(Vector2 origin, IReadOnlyList<Vector2> points)
        {
            int best = -1;
            float bestSq = float.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                float sq = (points[i] - origin).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = i; }
            }
            return best;
        }
    }
}
