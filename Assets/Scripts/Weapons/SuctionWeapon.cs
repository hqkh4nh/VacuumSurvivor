using UnityEngine;

namespace VacuumSurvivor
{
    /// The starting weapon: a ring around the robot that deals continuous damage to any trash
    /// inside it. Overdrive (set by the charging dock) doubles its reach and damage.
    public class SuctionWeapon : WeaponBase
    {
        [SerializeField] Transform ring;
        [SerializeField] float damageMultiplier = 1f;

        public float Overdrive { get; set; } = 1f;
        public float DamageMultiplier { get => damageMultiplier; set => damageMultiplier = value; }
        public Transform Ring { get => ring; set => ring = value; }

        int enemyMask;

        void Awake() => enemyMask = LayerMask.GetMask("Enemy");

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.Current != GameManager.State.Playing) return;

            SuctionTuning t = SuctionStats.For(data.dps, data.radius, Level, Overdrive);
            if (ring != null) ring.localScale = Vector3.one * (t.Radius * 2f);

            var hits = Physics2D.OverlapCircleAll(transform.position, t.Radius, enemyMask);
            float damage = t.Dps * damageMultiplier * Time.deltaTime;
            foreach (var h in hits)
            {
                var target = h.GetComponent<IDamageable>();
                if (target != null) target.TakeDamage(damage);
            }
        }
    }
}
