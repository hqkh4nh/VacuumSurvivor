using UnityEngine;

namespace VacuumSurvivor
{
    /// Maps a battery fraction (0..1) to its indicator colour. Shared by the robot's LED and the
    /// HUD bar so both always agree. Thresholds follow the GDD: >60% green, 30-60% yellow, <30% red.
    public static class BatteryColor
    {
        public static readonly Color Green = new Color(0.290f, 0.871f, 0.502f);  // #4ADE80
        public static readonly Color Yellow = new Color(0.980f, 0.800f, 0.082f); // #FACC15
        public static readonly Color Red = new Color(0.937f, 0.267f, 0.267f);    // #EF4444

        public static Color For(float percent)
        {
            if (percent > 0.6f) return Green;
            if (percent >= 0.3f) return Yellow;
            return Red;
        }
    }
}
