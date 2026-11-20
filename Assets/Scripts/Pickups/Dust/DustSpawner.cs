using UnityEngine;

namespace VacuumSurvivor
{
    /// Drops pooled dust wherever trash dies, scattered a little so a kill reads as a small burst.
    public class DustSpawner : MonoBehaviour
    {
        [SerializeField] GameObject dustPrefab;

        GameObjectPool pool;

        public void Configure(GameObject prefab) => dustPrefab = prefab;

        void Awake()
        {
            var holder = new GameObject("DustPool");
            holder.transform.SetParent(transform);
            pool = holder.AddComponent<GameObjectPool>();
            pool.Prefab = dustPrefab;
            pool.Prewarm();
        }

        void OnEnable() => EnemyBase.EnemyDied += Spawn;
        void OnDisable() => EnemyBase.EnemyDied -= Spawn;

        void Spawn(Vector2 pos, int amount)
        {
            int n = Mathf.Clamp(amount, 1, 6);
            for (int i = 0; i < n; i++)
            {
                Vector2 position = pos + Random.insideUnitCircle * 0.3f;
                if (RoomBounds.Instance != null) position = RoomBounds.Instance.Clamp(position, RoomBounds.RadiusOf(dustPrefab));
                var go = pool.Get(position);
                var dust = go.GetComponent<Dust>();
                dust.Pool = pool;
                dust.Init(1);
            }
        }
    }
}
