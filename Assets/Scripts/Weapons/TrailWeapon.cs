using UnityEngine;

namespace VacuumSurvivor
{
    /// Vệt lau sàn: periodically drops a wet patch at the robot's feet that damages trash crossing
    /// it. Per-level stats live here (like Orbit/Projectile); the WeaponData only names it.
    public class TrailWeapon : WeaponBase
    {
        [SerializeField] GameObjectPool pool;
        [SerializeField] float[] dps = { 5f, 7f, 9f, 10f, 12f };
        [SerializeField] float[] interval = { 0.6f, 0.55f, 0.5f, 0.45f, 0.4f };
        [SerializeField] float[] lifetime = { 3f, 3.5f, 4f, 4.5f, 5f };
        [SerializeField] float[] width = { 0.4f, 0.48f, 0.55f, 0.63f, 0.7f };

        public GameObjectPool Pool { get => pool; set => pool = value; }
        public override WeaponKind Kind => WeaponKind.Trail;

        float cooldownLeft;

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;
            if (!Owned || !playing || pool == null) return;

            cooldownLeft -= Time.deltaTime;
            if (cooldownLeft > 0f) return;

            TrailTuning t = TrailStats.For(dps, interval, lifetime, width, Level);
            var go = pool.Get(transform.position);
            var patch = go.GetComponent<TrailPatch>();
            if (patch != null) patch.Init(t.Dps, t.Lifetime, t.Width, pool);
            cooldownLeft = t.Interval;
        }
    }
}
