using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Fires water droplets at the nearest trash on a cooldown; extra shots fan out around the aim.
    public class ProjectileWeapon : WeaponBase
    {
        [SerializeField] GameObjectPool pool;
        [SerializeField] float[] damage = { 12f, 14f, 16f, 16f, 18f };
        [SerializeField] float[] cooldown = { 1.2f, 1.05f, 0.9f, 0.8f, 0.7f };
        [SerializeField] int[] shots = { 1, 1, 2, 2, 3 };
        [SerializeField] float[] range = { 6f, 6.5f, 7f, 7.5f, 8f };
        [SerializeField] float speed = 9f;
        [SerializeField] float spread = 30f;   // total fan angle across a multi-shot volley

        public GameObjectPool Pool { get => pool; set => pool = value; }
        public override WeaponKind Kind => WeaponKind.Projectile;

        int enemyMask;
        float cooldownLeft;
        readonly List<Vector2> points = new List<Vector2>();

        void Awake() => enemyMask = LayerMask.GetMask("Enemy");

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;
            if (!Owned || !playing || pool == null) return;

            cooldownLeft -= Time.deltaTime;
            if (cooldownLeft > 0f) return;

            ProjectileTuning t = ProjectileStats.For(damage, cooldown, shots, range, speed, Level);
            var enemies = Physics2D.OverlapCircleAll(transform.position, t.Range, enemyMask);
            if (enemies.Length == 0) return;

            points.Clear();
            foreach (var e in enemies) points.Add(e.transform.position);
            int idx = Targeting.Nearest(transform.position, points);
            if (idx < 0) return;

            Vector2 aim = ((Vector2)enemies[idx].transform.position - (Vector2)transform.position).normalized;
            Fire(aim, t);
            cooldownLeft = t.Cooldown;
        }

        void Fire(Vector2 aim, ProjectileTuning t)
        {
            float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            float step = t.Shots > 1 ? spread / (t.Shots - 1) : 0f;
            float start = baseAngle - (t.Shots > 1 ? spread * 0.5f : 0f);
            for (int i = 0; i < t.Shots; i++)
            {
                float a = (start + step * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var go = pool.Get(transform.position);
                var p = go.GetComponent<Projectile>();
                if (p != null) p.Launch(dir, t.Speed, t.Damage, pool);
            }
        }
    }
}
