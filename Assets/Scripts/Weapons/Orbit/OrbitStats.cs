using UnityEngine;

namespace VacuumSurvivor
{
    public struct OrbitTuning
    {
        public int Count;
        public float Dps;
        public float Radius;
        public float RotSpeed;
    }

    /// Pure resolver for the orbit weapon's per-level stats. Index 0 is level 1; out-of-range clamps.
    public static class OrbitStats
    {
        public static OrbitTuning For(int[] count, float[] dps, float[] radius, float[] rotSpeed, int level)
        {
            return new OrbitTuning
            {
                Count = count[Clamp(level, count.Length)],
                Dps = dps[Clamp(level, dps.Length)],
                Radius = radius[Clamp(level, radius.Length)],
                RotSpeed = rotSpeed[Clamp(level, rotSpeed.Length)],
            };
        }

        static int Clamp(int level, int length) => Mathf.Clamp(level - 1, 0, length - 1);
    }
}
