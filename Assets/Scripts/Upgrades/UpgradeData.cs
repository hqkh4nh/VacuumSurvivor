using UnityEngine;

namespace VacuumSurvivor
{
    public enum UpgradeType { SuctionLevel, MaxBattery, MoveSpeed, MagnetRadius, SuctionDamage, OrbitWeapon, ProjectileWeapon, TrailWeapon, LaserWeapon, PushWeapon }

    /// One pickable upgrade. valuePerLevel[0] is the effect of taking it the first time.
    [CreateAssetMenu(menuName = "VacuumSurvivor/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public UpgradeType type;
        public string title;
        [TextArea] public string description;
        public float[] valuePerLevel = { 1f };
        public int maxLevel = 5;
    }
}
