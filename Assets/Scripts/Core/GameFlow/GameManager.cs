using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace VacuumSurvivor
{
    /// Wave curve, boss, layout and survival timer for a campaign room.
    [System.Serializable]
    public class LevelConfig
    {
        public string name = "Phòng khách";
        public WaveData waves;
        public GameObject bossPrefab;
        public Color floorTint = Color.white;
        public Vector2 roomSize;
        public float duration = 180f;
    }

    /// Owns run state, the match clock and the win/lose transitions. Single instance per scene.
    public class GameManager : MonoBehaviour
    {
        public enum State { Playing, Paused, Won, Lost, Transition }

        public static GameManager Instance { get; private set; }

        public State Current { get; private set; } = State.Playing;
        public float Elapsed { get; private set; }

        bool bossPhase;
        public bool BossPhase => bossPhase;

        [SerializeField] float sliceDuration = 180f;
        public float SliceDuration { get => sliceDuration; set => sliceDuration = value; }

        [SerializeField] LevelConfig[] levels;
        public void Configure(LevelConfig[] campaignLevels) => levels = campaignLevels;

        const float TransitionDuration = 2f;
        Campaign campaign;
        float transitionTimer;

        Transform player;
        public Transform Player
        {
            get
            {
                if (player == null)
                {
                    var pc = FindFirstObjectByType<PlayerController>();
                    if (pc != null) player = pc.transform;
                }
                return player;
            }
            set => player = value;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            SetupCollisionMatrix();
        }

        void Start()
        {
            if (levels != null && levels.Length > 0)
            {
                campaign = new Campaign(levels.Length);
                ApplyLevel(campaign.Index);
            }
        }

        void Update()
        {
            // The between-level banner runs on unscaled time so it is unaffected by any timeScale.
            if (Current == State.Transition)
            {
                transitionTimer -= Time.unscaledDeltaTime;
                if (transitionTimer <= 0f)
                {
                    Current = State.Playing;
                    Time.timeScale = 1f;
                }
                return;
            }
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true) TogglePause();
            if (Current != State.Playing) return;
            Elapsed += Time.deltaTime;
            // Surviving the slice no longer wins outright: it summons the boss, and only killing
            // the boss wins.
            if (!bossPhase && SliceClock.IsCleared(Elapsed, sliceDuration))
            {
                bossPhase = true;
                GameEvents.RaiseBossPhase();
            }
        }

        void TogglePause()
        {
            if (Current == State.Paused)
            {
                Current = State.Playing;
                Time.timeScale = 1f;
            }
            else if (Current == State.Playing && Time.timeScale > 0f)
            {
                Current = State.Paused;
                Time.timeScale = 0f;
            }
        }

        /// A boss just died. Advance to the next room (recharged, weapons kept) or, if that was the
        /// final boss, win the whole game.
        public void BossDefeated()
        {
            if (Current != State.Playing) return;
            if (campaign == null || !campaign.BossDefeated())
            {
                Win();
                return;
            }
            bossPhase = false;
            Elapsed = 0f;
            var stats = Player != null ? Player.GetComponent<PlayerStats>() : null;
            if (stats != null) stats.RechargeFull();
            Current = State.Transition;
            transitionTimer = TransitionDuration;
            Time.timeScale = 0f;
            ApplyLevel(campaign.Index);
        }

        void ApplyLevel(int index)
        {
            var cfg = levels[index];
            sliceDuration = cfg.duration;

            var spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null) { spawner.Reconfigure(cfg.waves, cfg.bossPrefab); spawner.ClearLive(); }

            var floor = FindFirstObjectByType<ProceduralFloor>();
            if (floor != null) floor.GetComponent<SpriteRenderer>().color = cfg.floorTint;

            var room = RoomBounds.Instance;
            if (room != null && cfg.roomSize.x > 0f && cfg.roomSize.y > 0f)
            {
                room.Resize(cfg.roomSize);
                if (Player != null && Player.TryGetComponent<Rigidbody2D>(out var body))
                {
                    body.position = room.Clamp(body.position, RoomBounds.RadiusOf(body.gameObject));
                    body.linearVelocity = Vector2.zero;
                }
                foreach (var pickup in FindObjectsByType<Dust>(FindObjectsSortMode.None))
                    pickup.transform.position = room.Clamp(pickup.transform.position, RoomBounds.RadiusOf(pickup.gameObject));
                foreach (var pickup in FindObjectsByType<PowerUp>(FindObjectsSortMode.None))
                    pickup.transform.position = room.Clamp(pickup.transform.position, RoomBounds.RadiusOf(pickup.gameObject));
                foreach (var dock in FindObjectsByType<ChargingDock>(FindObjectsSortMode.None))
                    dock.transform.position = room.Clamp(dock.transform.position, RoomBounds.RadiusOf(dock.gameObject));
            }

            GameEvents.RaiseLevelChanged(index, cfg.name);
        }

        public static void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Win()
        {
            if (Current != State.Playing) return;
            Current = State.Won;
            Time.timeScale = 0f;
            GameEvents.RaiseWon();
        }

        public void Lose()
        {
            if (Current != State.Playing) return;
            Current = State.Lost;
            Time.timeScale = 0f;
            GameEvents.RaiseLost();
        }

        // Trash and dust pickups never physically collide; dust is gathered through triggers.
        static void SetupCollisionMatrix()
        {
            int enemy = LayerMask.NameToLayer("Enemy");
            int pickup = LayerMask.NameToLayer("Pickup");
            if (enemy >= 0 && pickup >= 0) Physics2D.IgnoreLayerCollision(pickup, enemy, true);
        }
    }
}
