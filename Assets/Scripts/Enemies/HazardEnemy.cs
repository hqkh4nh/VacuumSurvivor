using UnityEngine;

namespace VacuumSurvivor
{
    /// Static trash that sits still and hurts on contact: Cục LEGO (indestructible, self-removes
    /// after its lifetime) and Vũng nước (low health, "dries" when the suction weapon damages it).
    /// One class, split by EnemyData flags. Drops no dust.
    public class HazardEnemy : EnemyBase
    {
        float dieAt;

        protected override bool RecycleWhenFar => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            dieAt = data.lifetime > 0f ? Time.time + data.lifetime : 0f;
        }

        protected override void Move()
        {
            rb.linearVelocity = Vector2.zero;
            if (dieAt > 0f && Time.time >= dieAt) ReturnToPool();
        }

        public override void TakeDamage(float amount)
        {
            if (data.indestructible) return;   // Cục LEGO cannot be vacuumed up
            health.Damage(amount);
            if (health.IsDead) Die();          // Vũng nước dried out
        }
    }
}
