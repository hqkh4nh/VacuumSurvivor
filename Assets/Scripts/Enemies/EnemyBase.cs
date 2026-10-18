using UnityEngine;

namespace VacuumSurvivor
{
    /// Shared trash behaviour: health, damage flash, knockback, contact damage to the player, and
    /// death back into the pool. Each trash type only has to supply its own Move().
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [SerializeField] protected EnemyData data;
        [SerializeField] float knockback = 2f;

        /// Fired when any trash dies, with where it died and how much dust it drops.
        public static event System.Action<Vector2, int> EnemyDied;

        /// Set by the spawner so split children come from the pool. Returns the spawned enemy.
        public static System.Func<EnemyData, Vector2, GameObject> SpawnRequest;

        public EnemyData Data { get => data; set => data = value; }
        public GameObjectPool Pool { get; set; }

        protected Health health;
        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected Transform player;

        float flashUntil;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        protected virtual void OnEnable()
        {
            health = new Health(data.maxHealth);
            player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            sr.color = data.bodyColor;
        }

        void FixedUpdate()
        {
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.Player;
            Move();
        }

        void Update()
        {
            if (flashUntil > 0f && Time.time >= flashUntil)
            {
                sr.color = data.bodyColor;
                flashUntil = 0f;
            }
        }

        protected abstract void Move();

        public virtual void TakeDamage(float amount)
        {
            health.Damage(amount);
            sr.color = Color.white;
            flashUntil = Time.time + 0.1f;
            if (player != null)
            {
                Vector2 away = ((Vector2)transform.position - (Vector2)player.position).normalized;
                rb.AddForce(away * knockback, ForceMode2D.Impulse);
            }
            if (health.IsDead) Die();
        }

        protected virtual void Die()
        {
            EnemyDied?.Invoke(transform.position, data.dustValue);
            ReturnToPool();
        }

        protected void ReturnToPool()
        {
            if (Pool != null) Pool.Return(gameObject);
            else gameObject.SetActive(false);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            var ps = collision.collider.GetComponent<PlayerStats>();
            if (ps != null) ps.TakeContactDamage(data.contactDamage);
        }
    }
}
