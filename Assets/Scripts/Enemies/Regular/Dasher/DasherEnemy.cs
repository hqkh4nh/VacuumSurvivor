using UnityEngine;

namespace VacuumSurvivor
{
    /// Tất lẻ: stands still and telegraphs, then lunges in a straight line toward where the player
    /// was at the moment the dash began. Timing lives in DashCycle; this applies it to the body.
    public class DasherEnemy : EnemyBase
    {
        [SerializeField] Color telegraphColor = new Color(0.95f, 0.85f, 0.3f);

        DashCycle cycle;
        Vector2 dashDir;
        bool wasTelegraphing;

        protected override void OnEnable()
        {
            base.OnEnable();
            cycle = new DashCycle(data.aimTime, data.dashDuration, data.recoverTime);
            dashDir = Vector2.zero;
            wasTelegraphing = false;
        }

        protected override void Move()
        {
            var d = cycle.Tick(Time.fixedDeltaTime);
            if (d.StartedDash && player != null)
                dashDir = ((Vector2)player.position - rb.position).normalized;

            ApplyTelegraph(d.Telegraphing);
            rb.linearVelocity = d.Phase == DashCycle.Phase.Dashing ? dashDir * data.dashSpeed : Vector2.zero;
        }

        void ApplyTelegraph(bool telegraphing)
        {
            if (telegraphing) { sr.color = telegraphColor; wasTelegraphing = true; }
            else if (wasTelegraphing) { sr.color = data.bodyColor; wasTelegraphing = false; }
        }
    }
}
