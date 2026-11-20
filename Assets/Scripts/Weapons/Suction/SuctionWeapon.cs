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
        public override WeaponKind Kind => WeaponKind.Suction;

        int enemyMask;
        SpriteRenderer ringRenderer;
        Color ringColor;
        SuctionFlowEffect flowEffect;

        void Awake()
        {
            enemyMask = LayerMask.GetMask("Enemy");
            Owned = true;   // the starting weapon
            if (ring != null)
            {
                ringRenderer = ring.GetComponent<SpriteRenderer>();
                if (ringRenderer != null) ringColor = ringRenderer.color;
                flowEffect = ring.GetComponent<SuctionFlowEffect>();
                if (flowEffect == null) flowEffect = ring.gameObject.AddComponent<SuctionFlowEffect>();
            }
        }

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;
            if (!Owned || !playing)
            {
                if (ring != null) ring.gameObject.SetActive(false);
                return;
            }

            SuctionTuning t = SuctionStats.For(data.dps, data.radius, Level, Overdrive);
            if (ring != null)
            {
                ring.gameObject.SetActive(true);
                ring.localScale = Vector3.one * (t.Radius * 2f);
            }
            if (ringRenderer != null)
            {
                var color = ringColor;
                float strength = Overdrive > 1f ? 1.5f : 1f;
                color.a = Mathf.Clamp01(ringColor.a * 0.06f * (0.85f + 0.15f * Mathf.Sin(Time.time * 2.5f)) * strength);
                ringRenderer.color = color;
            }
            if (flowEffect != null) flowEffect.Render(t.Radius, Time.time, Overdrive);

            var hits = Physics2D.OverlapCircleAll(transform.position, t.Radius, enemyMask);
            float damage = t.Dps * damageMultiplier * Time.deltaTime;
            foreach (var h in hits)
            {
                var target = h.GetComponent<IDamageable>();
                if (target != null) target.TakeDamage(damage);
            }
        }

        void OnDisable()
        {
            if (ring != null) ring.gameObject.SetActive(false);
        }
    }
}
