using UnityEngine;

namespace VacuumSurvivor
{
    /// Còi báo động: on a cycle, shoves all nearby trash outward and deals light damage. An escape
    /// tool, not a damage dealer. The "stun" from the GDD is rendered as a strong knockback rather
    /// than a separate stun system. An optional ring pulses outward for feedback.
    public class PushWeapon : WeaponBase
    {
        [SerializeField] Transform ring;
        [SerializeField] float[] damage = { 5f, 8f, 10f, 12f, 15f };
        [SerializeField] float[] knockback = { 4f, 5f, 5.5f, 6f, 7f };
        [SerializeField] float[] cycle = { 12f, 11f, 10f, 9f, 8f };
        [SerializeField] float[] radius = { 4f, 4.5f, 5f, 5.5f, 6f };
        [SerializeField] float pulseTime = 0.35f;

        public Transform Ring { get => ring; set => ring = value; }
        public override WeaponKind Kind => WeaponKind.Push;

        int enemyMask;
        float cooldownLeft;
        float pulseLeft;
        float pulseRadius;

        void Awake() => enemyMask = LayerMask.GetMask("Enemy");

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;
            if (!Owned || !playing) { if (ring != null) ring.gameObject.SetActive(false); return; }

            PushTuning t = PushStats.For(damage, knockback, cycle, radius, Level);
            AnimatePulse(t.Radius);

            cooldownLeft -= Time.deltaTime;
            if (cooldownLeft > 0f) return;
            cooldownLeft = t.Cycle;

            var hits = Physics2D.OverlapCircleAll(transform.position, t.Radius, enemyMask);
            foreach (var h in hits)
            {
                Vector2 away = ((Vector2)h.transform.position - (Vector2)transform.position).normalized;
                var rb = h.attachedRigidbody;
                if (rb != null) rb.AddForce(away * t.Knockback, ForceMode2D.Impulse);
                var target = h.GetComponent<IDamageable>();
                if (target != null) target.TakeDamage(t.Damage);
            }
            pulseLeft = pulseTime;
            pulseRadius = t.Radius;
        }

        void AnimatePulse(float maxRadius)
        {
            if (ring == null) return;
            if (pulseLeft <= 0f) { ring.gameObject.SetActive(false); return; }
            pulseLeft -= Time.deltaTime;
            float k = 1f - (pulseLeft / pulseTime);   // 0 → 1 over the pulse
            ring.gameObject.SetActive(true);
            ring.localScale = Vector3.one * (pulseRadius * 2f * k);
        }
    }
}
