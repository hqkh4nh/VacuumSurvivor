using UnityEngine;

namespace VacuumSurvivor
{
    /// Boss Con mèo: a big, placid cat that wanders the room, sheds hairballs (Cục tóc rối) and
    /// telegraphs a slap at a nearby robot. Timing/decisions live in BossCatBrain; this drives the
    /// movement and applies the brain's decisions to the world. Killing it wins the slice.
    public class BossCat : EnemyBase
    {
        [SerializeField] EnemyData hairballData;
        [SerializeField] float slapRange = 3f;
        [SerializeField] float slapKnockback = 6f;
        [SerializeField] float slapDamage = 15f;
        [SerializeField] float windup = 0.7f;
        [SerializeField] float shedInterval = 3f;
        [SerializeField] float enragedShedInterval = 2f;
        [SerializeField] float slapCooldown = 6f;
        [SerializeField] float roamRadius = 6f;
        [SerializeField] float arrivePause = 2f;
        [SerializeField] Color telegraphColor = new Color(0.9f, 0.3f, 0.3f);

        public EnemyData HairballData { get => hairballData; set => hairballData = value; }
        public Color TelegraphColor { get => telegraphColor; set => telegraphColor = value; }

        BossCatBrain brain;
        Rigidbody2D playerRb;
        Vector2 roamTarget;
        float pauseTimer;
        bool wasTelegraphing;

        protected override bool RecycleWhenFar => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            brain = new BossCatBrain(shedInterval, enragedShedInterval, slapCooldown, slapRange, windup);
            pauseTimer = 0f;
            wasTelegraphing = false;
            PickRoamTarget();
            GameEvents.RaiseBossSpawned(data.maxHealth);
            GameEvents.RaiseBossHealth(health.Current, data.maxHealth);
        }

        protected override void Move()
        {
            float dt = Time.fixedDeltaTime;
            float dist = player != null ? Vector2.Distance(player.position, rb.position) : 999f;
            float hpFraction = data.maxHealth > 0f ? health.Current / data.maxHealth : 0f;

            var decision = brain.Tick(dt, dist, hpFraction);
            if (decision.Shed) Shed();
            if (decision.SlapHit) Slap();
            ApplyTelegraph(decision.Telegraphing);

            if (pauseTimer > 0f)
            {
                pauseTimer -= dt;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toTarget = roamTarget - rb.position;
            if (toTarget.sqrMagnitude <= 0.09f)
            {
                pauseTimer = arrivePause;
                PickRoamTarget();
                rb.linearVelocity = Vector2.zero;
                return;
            }
            rb.linearVelocity = toTarget.normalized * (data.moveSpeed * decision.SpeedMult);
        }

        void Shed()
        {
            if (SpawnRequest != null && hairballData != null)
                SpawnRequest(hairballData, rb.position);
        }

        void Slap()
        {
            if (player == null) return;
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();
            Vector2 away = ((Vector2)player.position - rb.position).normalized;
            // The player's velocity is rewritten every FixedUpdate from input, so a displacement
            // sticks where an impulse would not.
            if (playerRb != null) playerRb.position += away * slapKnockback;
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeContactDamage(slapDamage);
        }

        void ApplyTelegraph(bool telegraphing)
        {
            if (telegraphing) { sr.color = telegraphColor; wasTelegraphing = true; }
            else if (wasTelegraphing) { sr.color = data.bodyColor; wasTelegraphing = false; }
        }

        void PickRoamTarget()
        {
            Vector2 around = player != null ? (Vector2)player.position : Vector2.zero;
            roamTarget = around + Random.insideUnitCircle * roamRadius;
        }

        public override void TakeDamage(float amount)
        {
            base.TakeDamage(amount);
            GameEvents.RaiseBossHealth(health.Current, data.maxHealth);
        }

        protected override void Die()
        {
            base.Die(); // drops dust and deactivates (the boss has no pool)
            if (GameManager.Instance != null) GameManager.Instance.Win();
        }
    }
}
