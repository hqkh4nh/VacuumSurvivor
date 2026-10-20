using UnityEngine;

namespace VacuumSurvivor
{
    /// A pooled water droplet: flies straight, damages the first trash it hits, and returns to its
    /// pool on hit or after its lifetime. Lives on the Default layer so it still triggers against
    /// Enemy (GameManager disables Pickup↔Enemy collisions).
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float lifetime = 2f;

        Vector2 velocity;
        float damage;
        float dieAt;
        GameObjectPool pool;

        public void Launch(Vector2 direction, float speed, float dmg, GameObjectPool owner)
        {
            velocity = direction.normalized * speed;
            damage = dmg;
            pool = owner;
            dieAt = Time.time + lifetime;
            float a = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, a);
        }

        void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            if (Time.time >= dieAt) Recycle();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var target = other.GetComponent<IDamageable>();
            if (target == null) return;
            target.TakeDamage(damage);
            Recycle();
        }

        void Recycle()
        {
            if (pool != null) pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
