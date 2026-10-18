using UnityEngine;

namespace VacuumSurvivor
{
    /// Holds the robot's active weapons. In the vertical slice that is just the suction; upgrades
    /// level it up through here so the HUD and other systems get a single notification point.
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] SuctionWeapon suction;

        public SuctionWeapon Suction => suction;

        void Awake()
        {
            if (suction == null) suction = GetComponentInChildren<SuctionWeapon>();
        }

        void Start() => GameEvents.RaiseSuctionLevel(suction.Level);

        public void UpgradeSuction()
        {
            suction.LevelUp();
            GameEvents.RaiseSuctionLevel(suction.Level);
        }
    }
}
