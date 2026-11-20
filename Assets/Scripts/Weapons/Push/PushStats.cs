using UnityEngine;

namespace VacuumSurvivor
{
    public struct PushTuning
    {
        public float Damage;
        public float Knockback;
        public float Cycle;
        public float Radius;
    }

    /// Pure resolver for the alarm-horn weapon's per-level stats. Index 0 is level 1; out-of-range clamps.
    public static class PushStats
    {
        public static PushTuning For(float[] damage, float[] knockback, float[] cycle, float[] radius, int level)
        {
            return new PushTuning
            {
                Damage = damage[Clamp(level, damage.Length)],
                Knockback = knockback[Clamp(level, knockback.Length)],
                Cycle = cycle[Clamp(level, cycle.Length)],
                Radius = radius[Clamp(level, radius.Length)],
            };
        }

        static int Clamp(int level, int length) => Mathf.Clamp(level - 1, 0, length - 1);
    }
}
