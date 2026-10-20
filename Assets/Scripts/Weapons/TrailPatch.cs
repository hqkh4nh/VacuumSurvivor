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

        void Awake() => enemyMask = LayerMask.GetMask("Enemy");

        public void Init(float dps, float lifetime, float width, GameObjectPool owner)
        {
            this.dps = dps;
            radius = width;
            pool = owner;
            dieAt = Time.time + lifetime;
            transform.localScale = Vector3.one * (width * 2f);
        }

        void Update()
        {
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
