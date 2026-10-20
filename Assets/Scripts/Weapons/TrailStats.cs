using UnityEngine;

namespace VacuumSurvivor
{
    public struct TrailTuning
    {
        public float Dps;
        public float Interval;
        public float Lifetime;
        public float Width;
    }

    /// Pure resolver for the mop-trail weapon's per-level stats. Index 0 is level 1; out-of-range clamps.
    public static class TrailStats
    {
        public static TrailTuning For(float[] dps, float[] interval, float[] lifetime, float[] width, int level)
        {
            return new TrailTuning
            {
                Dps = dps[Clamp(level, dps.Length)],
                Interval = interval[Clamp(level, interval.Length)],
                Lifetime = lifetime[Clamp(level, lifetime.Length)],
                Width = width[Clamp(level, width.Length)],
            };
        }

        static int Clamp(int level, int length) => Mathf.Clamp(level - 1, 0, length - 1);
    }
}
