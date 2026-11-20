using UnityEngine;

namespace VacuumSurvivor
{
    /// Crawls straight at the player. Used for both Bụi bông and the faster Vụn bánh mì swarm,
    /// which differ only in their EnemyData.
    public class ChaserEnemy : EnemyBase
    {
        protected override void Move()
        {
            if (player == null) { rb.linearVelocity = Vector2.zero; return; }
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.linearVelocity = dir * data.moveSpeed;
        }
    }
}
