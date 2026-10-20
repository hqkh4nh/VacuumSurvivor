using UnityEngine;

namespace VacuumSurvivor
{
    /// Boss Máy hút bụi cũ: the old handheld vacuum back for revenge. Cycles three attacks — Hút
    /// (pulls the robot in), Thổi (coughs out a ring of Bụi bông) and Lao (telegraphs, then dashes
    /// across the room). Timing lives in BossVacuumBrain; this applies it. Killing it wins the game.
    public class BossVacuum : EnemyBase
    {
        [SerializeField] EnemyData blowData;
        [SerializeField] float pullSpeed = 3f;
        [SerializeField] int blowCount = 8;
        [SerializeField] float dashSpeed = 12f;
        [SerializeField] Color telegraphColor = new Color(0.95f, 0.3f, 0.3f);

        public EnemyData BlowData { get => blowData; set => blowData = value; }
        public Color TelegraphColor { get => telegraphColor; set => telegraphColor = value; }

        BossVacuumBrain brain;
        Rigidbody2D playerRb;
        Vector2 dashDir;
        bool wasTelegraphing;

        protected override bool RecycleWhenFar => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            brain = new BossVacuumBrain();
            wasTelegraphing = false;
            GameEvents.RaiseBossSpawned(data.maxHealth);
            GameEvents.RaiseBossHealth(health.Current, data.maxHealth);
        }

        protected override void Move()
        {
            float dt = Time.fixedDeltaTime;
            var d = brain.Tick(dt);

            if (d.Pulling) Pull(dt);
            if (d.BlowBurst) Blow();
            ApplyTelegraph(d.DashTelegraphing);

            if (d.DashActive) rb.linearVelocity = dashDir * dashSpeed;
            else if (d.Pulling) rb.linearVelocity = Vector2.zero;   // standing still while it inhales
            else rb.linearVelocity = ApproachVelocity();
        }

        void Pull(float dt)
        {
            if (player == null) return;
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb == null) return;
            Vector2 toBoss = (rb.position - (Vector2)player.position).normalized;
            playerRb.position += toBoss * (pullSpeed * dt);
        }

        void Blow()
        {
            if (SpawnRequest == null || blowData == null) return;
            for (int i = 0; i < blowCount; i++)
            {
                float a = i * (Mathf.PI * 2f / blowCount);
                Vector2 pos = rb.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.2f;
                SpawnRequest(blowData, pos);
            }
        }

        void ApplyTelegraph(bool telegraphing)
        {
            if (telegraphing)
            {
                if (!wasTelegraphing && player != null)
                    dashDir = ((Vector2)player.position - rb.position).normalized;
                sr.color = telegraphColor;
                wasTelegraphing = true;
            }
            else if (wasTelegraphing)
            {
                sr.color = data.bodyColor;
                wasTelegraphing = false;
            }
        }

        Vector2 ApproachVelocity()
        {
            if (player == null) return Vector2.zero;
            return ((Vector2)player.position - rb.position).normalized * data.moveSpeed;
        }

        public override void TakeDamage(float amount)
        {
            base.TakeDamage(amount);
            GameEvents.RaiseBossHealth(health.Current, data.maxHealth);
        }

        protected override void Die()
        {
            base.Die();
            if (GameManager.Instance != null) GameManager.Instance.BossDefeated();
        }
    }
}
