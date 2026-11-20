using UnityEngine;

namespace VacuumSurvivor
{
    public struct LaserTuning
    {
        public float Damage;
        public float Cycle;
        public int Beams;
        public float Range;
    }

    /// Pure resolver for the LIDAR weapon's per-level stats. Index 0 is level 1; out-of-range clamps.
    public static class LaserStats
    {
        public static LaserTuning For(float[] damage, float[] cycle, int[] beams, float range, int level)
        {
            return new LaserTuning
            {
                Damage = damage[Clamp(level, damage.Length)],
                Cycle = cycle[Clamp(level, cycle.Length)],
                Beams = beams[Clamp(level, beams.Length)],
                Range = range,
            };
        }

        static int Clamp(int level, int length) => Mathf.Clamp(level - 1, 0, length - 1);
    }
}
