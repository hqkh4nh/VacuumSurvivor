using UnityEngine;

namespace VacuumSurvivor
{
    public struct ProjectileTuning
    {
        public float Damage;
        public float Cooldown;
        public int Shots;
        public float Range;
        public float Speed;
    }

    /// Pure resolver for the projectile weapon's per-level stats. Index 0 is level 1; range clamps.
    public static class ProjectileStats
    {
        public static ProjectileTuning For(float[] damage, float[] cooldown, int[] shots, float[] range, float speed, int level)
        {
            return new ProjectileTuning
            {
                Damage = damage[Clamp(level, damage.Length)],
                Cooldown = cooldown[Clamp(level, cooldown.Length)],
                Shots = shots[Clamp(level, shots.Length)],
                Range = range[Clamp(level, range.Length)],
                Speed = speed,
            };
        }

        static int Clamp(int level, int length) => Mathf.Clamp(level - 1, 0, length - 1);
    }
}
