using UnityEngine;

namespace VacuumSurvivor
{
    /// Dây sạc: crawls toward the player along a weaving path and, on contact, wraps the robot's
    /// wheels so it cannot move for a moment.
    public class SnakeEnemy : EnemyBase
    {
        float startTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            startTime = Time.time;
        }

        protected override void Move()
        {
            if (player == null) { rb.linearVelocity = Vector2.zero; return; }
            Vector2 toward = (Vector2)player.position - rb.position;
            Vector2 dir = SnakeWeave.Direction(toward, Time.time - startTime, data.weaveAmp, data.weaveFreq);
            rb.linearVelocity = dir * data.moveSpeed;
        }

        protected override void OnContactPlayer(PlayerStats stats)
        {
            var pc = stats.GetComponent<PlayerController>();
            if (pc != null) pc.Root(data.rootDuration);
        }
    }
}
