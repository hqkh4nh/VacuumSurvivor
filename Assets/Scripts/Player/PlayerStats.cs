using UnityEngine;

namespace VacuumSurvivor
{
    /// The robot's battery (its health). Drains continuously, loses charge on trash contact with
    /// a brief invulnerability window, recharges on the dock, and ends the run when empty.
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] float maxBattery = 100f;
        [SerializeField] float drainPerSecond = 0.25f;
        [SerializeField] float invulnerability = 0.4f;

        BatteryModel battery;
        float lastHitTime = -999f;

        public float Battery => battery.Current;
        public float MaxBattery => battery.Max;
        public float Percent => battery.Max > 0f ? battery.Current / battery.Max : 0f;

        readonly LevelProgress progress = new LevelProgress();
        public int Level => progress.Level;
        public int Dust => progress.Dust;
        public int DustForNext => progress.DustForNext;

        void Awake() => battery = new BatteryModel(maxBattery);

        void Start()
        {
            GameEvents.RaiseBattery(battery.Current, battery.Max);
            GameEvents.RaiseDust(progress.Dust, progress.DustForNext);
        }

        void Update()
        {
            if (!IsPlaying) return;
            battery.Tick(Time.deltaTime, drainPerSecond);
            GameEvents.RaiseBattery(battery.Current, battery.Max);
            if (battery.IsDead) Die();
        }

        public void TakeContactDamage(float amount)
        {
            if (!IsPlaying || Time.time - lastHitTime < invulnerability) return;
            lastHitTime = Time.time;
            battery.Damage(amount);
            GameEvents.RaiseBattery(battery.Current, battery.Max);
            if (battery.IsDead) Die();
        }

        public void Charge(float perSecond)
        {
            battery.Charge(Time.deltaTime, perSecond);
            GameEvents.RaiseBattery(battery.Current, battery.Max);
        }

        public void AddMaxBattery(float amount)
        {
            battery.AddMax(amount, true);
            GameEvents.RaiseBattery(battery.Current, battery.Max);
        }

        public void AddDust(int amount)
        {
            int gained = progress.AddDust(amount);
            GameEvents.RaiseDust(progress.Dust, progress.DustForNext);
            for (int i = 0; i < gained; i++) GameEvents.RaisePlayerLevel(progress.Level);
        }

        static bool IsPlaying =>
            GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;

        void Die()
        {
            if (GameManager.Instance != null) GameManager.Instance.Lose();
        }
    }
}
