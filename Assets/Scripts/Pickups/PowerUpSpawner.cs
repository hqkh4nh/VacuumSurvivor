using UnityEngine;

namespace VacuumSurvivor
{
    /// Rolls for a floor power-up whenever trash dies and drops one at the kill site.
    public class PowerUpSpawner : MonoBehaviour
    {
        [SerializeField] GameObject powerUpPrefab;
        [SerializeField] float dropChance = 0.05f;
        [SerializeField] float batteryAmount = 30f;
        [SerializeField] float bombRadius = 5f;
        [SerializeField] float bombDamage = 60f;
        [SerializeField] float lifetime = 12f;

        GameObjectPool pool;
        System.Random rng;

        public void Configure(GameObject prefab) => powerUpPrefab = prefab;

        void Awake()
        {
            rng = new System.Random();
            var holder = new GameObject("PowerUpPool");
            holder.transform.SetParent(transform);
            pool = holder.AddComponent<GameObjectPool>();
            pool.Prefab = powerUpPrefab;
        }

        void OnEnable() => EnemyBase.EnemyDied += OnDied;
        void OnDisable() => EnemyBase.EnemyDied -= OnDied;

        void OnDied(Vector2 pos, int dustAmount)
        {
            if (powerUpPrefab == null) return;
            if (!PowerUpRoll.ShouldDrop(rng.NextDouble(), dropChance)) return;

            var go = pool.Get(pos);
            var pu = go.GetComponent<PowerUp>();
            pu.Pool = pool;
            pu.Init(PowerUpRoll.Pick(rng.NextDouble()), batteryAmount, bombRadius, bombDamage, lifetime);
        }
    }
}
