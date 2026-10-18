using UnityEngine;
using UnityEngine.InputSystem;

namespace VacuumSurvivor
{
    /// Reads WASD/arrow input and drives the robot with Rigidbody2D velocity.
    /// SpeedMultiplier lets other systems (the charging dock) slow the robot without
    /// touching its base speed.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;

        public float SpeedMultiplier { get; set; } = 1f;
        public Vector2 Facing { get; private set; } = Vector2.down;
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

        Rigidbody2D rb;
        Vector2 input;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (GameManager.Instance != null) GameManager.Instance.Player = transform;
        }

        void Update()
        {
            input = ReadMove();
            if (input.sqrMagnitude > 0.01f) Facing = input.normalized;
        }

        void FixedUpdate()
        {
            rb.linearVelocity = input.normalized * (moveSpeed * SpeedMultiplier);
        }

        static Vector2 ReadMove()
        {
            var k = Keyboard.current;
            if (k == null) return Vector2.zero;
            float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                    - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
            float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                    - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
            return new Vector2(x, y);
        }
    }
}
