using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Holds the robot's weapons. New weapons are acquired, then levelled, through here by kind, so
    /// the HUD and other systems get a single notification point.
    public class WeaponManager : MonoBehaviour
    {
        readonly List<WeaponBase> weapons = new List<WeaponBase>();
        SuctionWeapon suction;

        public SuctionWeapon Suction => suction;

        void Awake()
        {
            weapons.Clear();
            weapons.AddRange(GetComponentsInChildren<WeaponBase>(true));
            foreach (var w in weapons)
                if (w is SuctionWeapon s) suction = s;
        }

        void Start() => GameEvents.RaiseWeapons(Summary());

        /// First pick of a weapon's upgrade card acquires it; later picks level it up to its max.
        public void AcquireOrUpgrade(WeaponKind kind)
        {
            foreach (var w in weapons)
            {
                if (w.Kind != kind) continue;
                if (!w.Owned) w.Owned = true;
                else w.LevelUp();
                break;
            }
            GameEvents.RaiseWeapons(Summary());
        }

        public void UpgradeSuction() => AcquireOrUpgrade(WeaponKind.Suction);

        public string Summary()
        {
            var sb = new StringBuilder();
            foreach (var w in weapons)
            {
                if (!w.Owned) continue;
                if (sb.Length > 0) sb.Append("  ");
                string name = w.Data != null ? w.Data.weaponName : w.Kind.ToString();
                sb.Append(name).Append(" Lv").Append(w.Level);
            }
            return sb.ToString();
        }
    }
}
