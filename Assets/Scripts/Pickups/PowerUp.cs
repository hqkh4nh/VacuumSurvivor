using UnityEngine;

namespace VacuumSurvivor
{
    public enum PowerUpKind { Battery, Magnet, Bomb }

    /// A floor power-up dropped by dying trash. Walk over it to trigger its effect, then it returns
    /// to its pool. Uncollected ones despawn after a lifetime so the floor doesn't fill up.
    public class PowerUp : MonoBehaviour
    {
        public GameObjectPool Pool { get; set; }

        PowerUpKind kind;
        float batteryAmount, bombRadius, bombDamage, lifetime, age;
        int enemyMask;
        ProceduralSprite art;

        void Awake()
        {
            enemyMask = LayerMask.GetMask("Enemy");
            art = GetComponent<ProceduralSprite>();
        }

        public void Init(PowerUpKind k, float battery, float radius, float damage, float life)
        {
            kind = k;
            batteryAmount = battery;
            bombRadius = radius;
            bombDamage = damage;
            lifetime = life;
            age = 0f;
            if (art != null) { art.Kind = SpriteFor(k); art.Apply(); }
        }

        void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime) Despawn();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var stats = other.GetComponent<PlayerStats>();
            if (stats == null) return;
            Apply(stats);
            GameEvents.RaisePowerUp(kind);
            Despawn();
        }

        void Apply(PlayerStats stats)
        {
            switch (kind)
            {
                case PowerUpKind.Battery:
                    stats.AddBattery(batteryAmount);
                    break;
                case PowerUpKind.Magnet:
                    foreach (var d in Object.FindObjectsByType<Dust>(FindObjectsSortMode.None))
                    {
                        if (!d.gameObject.activeInHierarchy) continue;
                        stats.AddDust(d.Value);
                        d.ReturnToPool();
                    }
                    break;
                case PowerUpKind.Bomb:
                    foreach (var h in Physics2D.OverlapCircleAll(transform.position, bombRadius, enemyMask))
                    {
                        var target = h.GetComponent<IDamageable>();
                        if (target != null) target.TakeDamage(bombDamage);
                    }
                    break;
            }
        }

        static SpriteKind SpriteFor(PowerUpKind k)
        {
            switch (k)
            {
                case PowerUpKind.Battery: return SpriteKind.Battery;
                case PowerUpKind.Magnet: return SpriteKind.Magnet;
                default: return SpriteKind.Bomb;
            }
        }

        void Despawn()
        {
            if (Pool != null) Pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
