using UnityEngine;

namespace VacuumSurvivor
{
    /// Drops a charging dock near the player on a fixed cadence.
    public class DockSpawner : MonoBehaviour
    {
        [SerializeField] GameObject dockPrefab;
        [SerializeField] float interval = 30f;
        [SerializeField] float spawnRadius = 4f;

        GameObjectPool pool;
        float timer;
        Transform player;

        public void Configure(GameObject prefab) => dockPrefab = prefab;

        void Awake()
        {
            var holder = new GameObject("DockPool");
            holder.transform.SetParent(transform);
            pool = holder.AddComponent<GameObjectPool>();
            pool.Prefab = dockPrefab;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Current != GameManager.State.Playing) return;
            if (player == null) player = GameManager.Instance.Player;
            if (player == null) return;

            timer += Time.deltaTime;
            if (timer < interval) return;
            timer = 0f;

            Vector2 pos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
            var go = pool.Get(pos);
            go.GetComponent<ChargingDock>().Pool = pool;
        }
    }
}
