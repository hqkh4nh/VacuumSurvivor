using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Turns each level-up into a paused choice of three upgrades and applies the pick. Multiple
    /// level-ups queue and show one panel after another.
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] List<UpgradeData> catalog = new List<UpgradeData>();
        [SerializeField] LevelUpPanel panel;

        readonly Dictionary<UpgradeData, int> levels = new Dictionary<UpgradeData, int>();
        int pending;
        System.Random rng;

        public void Configure(List<UpgradeData> upgrades, LevelUpPanel levelUpPanel)
        {
            catalog = upgrades;
            panel = levelUpPanel;
        }

        void Awake() => rng = new System.Random();

        void OnEnable() => GameEvents.PlayerLevelChanged += OnLevelUp;
        void OnDisable() => GameEvents.PlayerLevelChanged -= OnLevelUp;

        void OnLevelUp(int level)
        {
            pending++;
            TryShowNext();
        }

        void TryShowNext()
        {
            if (panel == null || panel.IsOpen || pending <= 0) return;
            if (GameManager.Instance != null && GameManager.Instance.Current != GameManager.State.Playing) return;
            pending--;

            var options = UpgradeRoll.Pick(catalog, LevelOf, 3, rng);
            if (options.Count == 0) return;   // everything maxed: nothing to offer
            panel.Show(options, i => Apply(options[i]));
        }

        int LevelOf(UpgradeData u) => levels.TryGetValue(u, out int l) ? l : 0;

        void Apply(UpgradeData u)
        {
            int lvl = LevelOf(u);
            float value = u.valuePerLevel[Mathf.Min(lvl, u.valuePerLevel.Length - 1)];
            ApplyEffect(u.type, value);
            levels[u] = lvl + 1;

            // Don't un-pause a run that ended (level-up and death on the same frame).
            if (GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing)
                Time.timeScale = 1f;
            TryShowNext();
        }

        void ApplyEffect(UpgradeType type, float value)
        {
            Transform p = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (p == null) return;

            switch (type)
            {
                case UpgradeType.SuctionLevel:
                    p.GetComponent<WeaponManager>()?.UpgradeSuction();
                    break;
                case UpgradeType.MaxBattery:
                    p.GetComponent<PlayerStats>()?.AddMaxBattery(value);
                    break;
                case UpgradeType.MoveSpeed:
                    { var pc = p.GetComponent<PlayerController>(); if (pc != null) pc.MoveSpeed += value; break; }
                case UpgradeType.MagnetRadius:
                    { var m = p.GetComponent<PlayerMagnet>(); if (m != null) m.RadiusMultiplier += value; break; }
                case UpgradeType.SuctionDamage:
                    { var s = p.GetComponentInChildren<SuctionWeapon>(); if (s != null) s.DamageMultiplier += value; break; }
            }
        }
    }
}
