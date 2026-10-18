using UnityEngine;

namespace VacuumSurvivor
{
    /// Pulls nearby dust toward the robot and collects it. The radius grows with the Nam châm
    /// upgrade via RadiusMultiplier.
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMagnet : MonoBehaviour
    {
        [SerializeField] float baseRadius = 1.5f;
        [SerializeField] float collectDistance = 0.35f;

        public float RadiusMultiplier { get; set; } = 1f;
        public float Radius => baseRadius * RadiusMultiplier;

        int pickupMask;
        PlayerStats stats;

        void Awake()
        {
            pickupMask = LayerMask.GetMask("Pickup");
            stats = GetComponent<PlayerStats>();
        }

        void Update()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, Radius, pickupMask);
            foreach (var h in hits)
            {
                var dust = h.GetComponent<Dust>();
                if (dust == null) continue;
                dust.PullToward(transform.position);
                if (((Vector2)h.transform.position - (Vector2)transform.position).sqrMagnitude <= collectDistance * collectDistance)
                {
                    stats.AddDust(dust.Value);
                    dust.ReturnToPool();
                }
            }
        }
    }
}
