using UnityEngine;

namespace VacuumSurvivor
{
    /// Watches a run and, when it ends, writes its score to the high-score board. The end screen
    /// reads the finalised stats from here.
    public class RunTracker : MonoBehaviour
    {
        public static RunTracker Instance { get; private set; }

        int kills;
        int dust;
        float seconds;

        public RunStats Last { get; private set; }
        public bool Won { get; private set; }
        public int Level { get; private set; }
        public bool HasResult { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            EnemyBase.EnemyDied += OnEnemyDied;
            GameEvents.Won += OnWon;
            GameEvents.Lost += OnLost;
        }

        void OnDisable()
        {
            EnemyBase.EnemyDied -= OnEnemyDied;
            GameEvents.Won -= OnWon;
            GameEvents.Lost -= OnLost;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.Current == GameManager.State.Playing)
                seconds += Time.deltaTime;
        }

        void OnEnemyDied(Vector2 pos, int droppedDust)
        {
            kills++;
            dust += droppedDust;
        }

        void OnWon() => Finalize(true);
        void OnLost() => Finalize(false);

        void Finalize(bool won)
        {
            if (HasResult) return;
            Won = won;
            Level = PlayerLevel();
            Last = new RunStats { dust = dust, kills = kills, seconds = Mathf.FloorToInt(seconds) };
            HasResult = true;

            var entry = new HighScoreEntry
            {
                score = Last.Score(won),
                seconds = Last.seconds,
                level = Level,
                date = System.DateTime.Now.ToString("yyyy-MM-dd"),
            };
            SaveSystem.Save(HighScoreTable.Insert(SaveSystem.Load(), entry));
        }

        static int PlayerLevel()
        {
            var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
            var stats = p != null ? p.GetComponent<PlayerStats>() : null;
            return stats != null ? stats.Level : 1;
        }
    }
}
