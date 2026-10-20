using UnityEngine;
using UnityEngine.SceneManagement;

namespace VacuumSurvivor
{
    /// Owns run state, the match clock and the win/lose transitions. Single instance per scene.
    public class GameManager : MonoBehaviour
    {
        public enum State { Playing, Paused, Won, Lost }

        public static GameManager Instance { get; private set; }

        public State Current { get; private set; } = State.Playing;
        public float Elapsed { get; private set; }

        bool bossPhase;
        public bool BossPhase => bossPhase;

        [SerializeField] float sliceDuration = 180f;
        public float SliceDuration { get => sliceDuration; set => sliceDuration = value; }

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

        void Update()
        {
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
