using UnityEngine;

namespace VacuumSurvivor
{
    /// A pooled wet patch left by the mop-trail weapon: damages trash standing in it, then returns
    /// to its pool after its lifetime. Continuous DPS, matching the suction model.
    public class TrailPatch : MonoBehaviour
    {
        int enemyMask;
        float dps;
        float radius;
        float dieAt;
        GameObjectPool pool;
        SpriteRenderer surface;
        Color surfaceColor;
        float duration;

        void Awake()
        {
            enemyMask = LayerMask.GetMask("Enemy");
            surface = GetComponent<SpriteRenderer>();
            if (surface != null) surfaceColor = surface.color;
        }

        public void Init(float dps, float lifetime, float width, GameObjectPool owner)
        {
            this.dps = dps;
            radius = width;
            pool = owner;
            duration = lifetime;
            dieAt = Time.time + lifetime;
            if (surface != null) surface.color = surfaceColor;
            transform.localScale = Vector3.one * (width * 2f);
        }

        void Update()
        {
            if (surface != null)
            {
                var color = surfaceColor;
                float remaining = Mathf.Max(0f, dieAt - Time.time);
                color.a *= Mathf.Clamp01(remaining / Mathf.Max(0.01f, duration * 0.25f));
                surface.color = color;
            }
            float damage = dps * Time.deltaTime;
            var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
            foreach (var h in hits)
            {
                var target = h.GetComponent<IDamageable>();
                if (target != null) target.TakeDamage(damage);
            }
            if (Time.time >= dieAt) Recycle();
        }

        void Recycle()
        {
            if (pool != null) pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
