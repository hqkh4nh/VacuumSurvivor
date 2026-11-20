using UnityEngine;

namespace VacuumSurvivor
{
    /// Pure heading for Dây sạc: it crawls toward the player but weaves side to side, so it reads as
    /// a wriggling cord rather than a straight chaser. Returns a unit direction.
    public static class SnakeWeave
    {
        public static Vector2 Direction(Vector2 toward, float time, float amp, float freq)
        {
            Vector2 forward = toward.sqrMagnitude > 0.0001f ? toward.normalized : Vector2.right;
            Vector2 perp = new Vector2(-forward.y, forward.x);
            Vector2 dir = forward + perp * (amp * Mathf.Sin(freq * time));
            return dir.normalized;
        }
    }
}
