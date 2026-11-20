using UnityEngine;

namespace VacuumSurvivor
{
    /// Per-level tuning for a weapon, edited in the Inspector. Index 0 is level 1.
    [CreateAssetMenu(menuName = "VacuumSurvivor/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName = "Weapon";
        public int maxLevel = 5;
        public float[] dps = { 4f, 5.5f, 7f, 8.5f, 10f };
        public float[] radius = { 1.5f, 1.8f, 2.1f, 2.45f, 2.8f };
    }
}
