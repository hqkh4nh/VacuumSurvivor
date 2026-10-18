using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Spawns pooled trash in a ring around the player, following the level's WaveData curve and
    /// respecting the live-count cap. Also serves split children from the same pools.
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] WaveData waves;
        [SerializeField] GameObject[] enemyPrefabs;
        [SerializeField] float spawnRadius = 8f;

        readonly Dictionary<EnemyData, GameObjectPool> pools = new Dictionary<EnemyData, GameObjectPool>();
        int activeCount;
        float timer;
        Transform player;

        public int ActiveCount => activeCount;

        public void Configure(WaveData waveData, GameObject[] prefabs)
        {
            waves = waveData;
            enemyPrefabs = prefabs;
        }

        void Awake()
        {
            foreach (var prefab in enemyPrefabs)
            {
                var data = prefab.GetComponent<EnemyBase>().Data;
                if (data == null || pools.ContainsKey(data)) continue;
                var holder = new GameObject("Pool_" + data.enemyName);
                holder.transform.SetParent(transform);
                var pool = holder.AddComponent<GameObjectPool>();
                pool.Prefab = prefab;
                pool.Prewarm();
                pools[data] = pool;
            }
            EnemyBase.SpawnRequest = SpawnAt;
        }

        void OnEnable() => EnemyBase.EnemyDied += OnEnemyDied;

        void OnDisable()
        {
            EnemyBase.EnemyDied -= OnEnemyDied;
            if (EnemyBase.SpawnRequest == SpawnAt) EnemyBase.SpawnRequest = null;
        }

        void OnEnemyDied(Vector2 pos, int dust)
        {
            activeCount--;
            if (activeCount < 0) activeCount = 0;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Current != GameManager.State.Playing) return;
            if (player == null) player = GameManager.Instance.Player;
            if (player == null || waves == null) return;

            var wave = WaveSchedule.Current(GameManager.Instance.Elapsed, waves.entries);
            if (wave == null || wave.types == null || wave.types.Length == 0) return;

            timer += Time.deltaTime;
            if (timer < wave.spawnInterval || activeCount >= wave.maxAlive) return;
            timer = 0f;

            var type = wave.types[Random.Range(0, wave.types.Length)];
            if (type == null) return;
            int burst = type.swarmMax > 0 ? Random.Range(type.swarmMin, type.swarmMax + 1) : 1;
            for (int i = 0; i < burst && activeCount < wave.maxAlive; i++)
                SpawnAt(type, RingPosition());
        }

        Vector2 RingPosition()
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir == Vector2.zero) dir = Vector2.up;
            return (Vector2)player.position + dir * spawnRadius;
        }

        public GameObject SpawnAt(EnemyData data, Vector2 pos)
        {
            if (!pools.TryGetValue(data, out var pool)) return null;
            var obj = pool.Get(pos);
            obj.GetComponent<EnemyBase>().Pool = pool;
            activeCount++;
            return obj;
        }
    }
}
