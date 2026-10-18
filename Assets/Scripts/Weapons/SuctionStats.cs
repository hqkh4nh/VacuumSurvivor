using UnityEngine;

namespace VacuumSurvivor
{
    public struct SuctionTuning
    {
        public float Dps;
        public float Radius;
    }

    /// Pure resolver for the suction weapon's damage and radius at a given level, with an overdrive
    /// multiplier (the charging dock passes 2). Isolated so it is unit-testable in EditMode.
    public static class SuctionStats
    {
        public static SuctionTuning For(float[] dps, float[] radius, int level, float overdrive)
        {
            int di = Mathf.Clamp(level - 1, 0, dps.Length - 1);
            int ri = Mathf.Clamp(level - 1, 0, radius.Length - 1);
            return new SuctionTuning { Dps = dps[di] * overdrive, Radius = radius[ri] * overdrive };
        }
    }
}
