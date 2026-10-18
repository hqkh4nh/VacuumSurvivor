using UnityEngine;

namespace VacuumSurvivor
{
    /// Owns run state, the match clock and the win/lose transitions. Single instance per scene.
    public class GameManager : MonoBehaviour
    {
        public enum State { Playing, Paused, Won, Lost }

        public static GameManager Instance { get; private set; }

        public State Current { get; private set; } = State.Playing;
        public Transform Player { get; set; }
        public float Elapsed { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            SetupCollisionMatrix();
        }

        void Update()
        {
            if (Current == State.Playing) Elapsed += Time.deltaTime;
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
