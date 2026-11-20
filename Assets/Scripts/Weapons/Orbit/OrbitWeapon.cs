using UnityEngine;

namespace VacuumSurvivor
{
    /// Brooms that orbit the robot; each broom tip deals continuous damage to trash it overlaps.
    /// The GDD describes per-touch damage; continuous DPS at the tips matches the suction model and
    /// avoids per-enemy hit bookkeeping.
    public class OrbitWeapon : WeaponBase
    {
        [SerializeField] Transform pivot;
        [SerializeField] Transform[] brooms;
        [SerializeField] int[] count = { 1, 2, 3, 3, 4 };
        [SerializeField] float[] dps = { 6f, 8f, 10f, 12f, 14f };
        [SerializeField] float[] radius = { 1.8f, 1.9f, 2.0f, 2.1f, 2.2f };
        [SerializeField] float[] rotSpeed = { 120f, 140f, 160f, 180f, 200f };
        [SerializeField] float tipRadius = 0.4f;

        public Transform Pivot { get => pivot; set => pivot = value; }
        public Transform[] Brooms { get => brooms; set => brooms = value; }
        public override WeaponKind Kind => WeaponKind.Orbit;

        int enemyMask;
        float angle;

        void Awake() => enemyMask = LayerMask.GetMask("Enemy");

        void OnEnable() => Layout();
        protected override void OnLevelChanged() => Layout();

        /// Spread the active brooms evenly around the pivot for the current level; hide the rest.
        void Layout()
        {
            if (brooms == null) return;
            OrbitTuning t = OrbitStats.For(count, dps, radius, rotSpeed, Level);
            for (int i = 0; i < brooms.Length; i++)
            {
                if (brooms[i] == null) continue;
                bool on = i < t.Count;
                brooms[i].gameObject.SetActive(on);
                if (!on) continue;
                float a = (360f / t.Count) * i * Mathf.Deg2Rad;
                brooms[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * t.Radius;
            }
        }

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;
            if (!Owned || !playing)
            {
                if (pivot != null) pivot.gameObject.SetActive(false);
                return;
            }

            OrbitTuning t = OrbitStats.For(count, dps, radius, rotSpeed, Level);
            if (pivot != null)
            {
                if (!pivot.gameObject.activeSelf) { pivot.gameObject.SetActive(true); Layout(); }
                angle += t.RotSpeed * Time.deltaTime;
                pivot.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (brooms == null) return;
            float damage = t.Dps * Time.deltaTime;
            for (int i = 0; i < brooms.Length && i < t.Count; i++)
            {
                if (brooms[i] == null || !brooms[i].gameObject.activeSelf) continue;
                var hits = Physics2D.OverlapCircleAll(brooms[i].position, tipRadius, enemyMask);
                foreach (var h in hits)
                {
                    var target = h.GetComponent<IDamageable>();
                    if (target != null) target.TakeDamage(damage);
                }
            }
        }
    }
}
