using UnityEngine;

namespace VacuumSurvivor
{
    /// Tia quét LIDAR: on a cycle, sweeps rotating beams a full turn, damaging trash the beam
    /// crosses. Continuous damage within a narrow wedge during the sweep, matching the suction model
    /// rather than per-pass hit bookkeeping. Beams are line renderers created at runtime.
    public class LaserWeapon : WeaponBase
    {
        [SerializeField] float[] damage = { 25f, 30f, 35f, 40f, 45f };
        [SerializeField] float[] cycle = { 5f, 4.5f, 4f, 3.5f, 3f };
        [SerializeField] int[] beams = { 1, 1, 2, 2, 3 };
        [SerializeField] float range = 6f;
        [SerializeField] float sweepDuration = 0.5f;
        [SerializeField] float wedge = 18f;   // half-angle (deg) that counts as "on the beam"

        public override WeaponKind Kind => WeaponKind.Laser;

        int enemyMask;
        float cooldownLeft;
        float sweepLeft;
        float angle;
        LineRenderer[] lines;
        Material material;

        void Awake()
        {
            enemyMask = LayerMask.GetMask("Enemy");
            BuildLines(3);
        }

        void BuildLines(int n)
        {
            if (lines != null) return;
            lines = new LineRenderer[n];
            material = new Material(Resources.Load<Material>("Materials/WeaponStroke"));
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Beam" + i);
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = material;
                lr.useWorldSpace = true;
                lr.positionCount = 2;
                lr.startWidth = 0.18f;
                lr.endWidth = 0.075f;
                lr.numCapVertices = 6;
                lr.sortingOrder = 15;
                lr.startColor = new Color(0.22f, 1f, 0.67f, 1f);
                lr.endColor = new Color(0.55f, 1f, 0.8f, 0.85f);
                lr.enabled = false;
                lines[i] = lr;
            }
        }

        void Update()
        {
            bool playing = GameManager.Instance == null || GameManager.Instance.Current == GameManager.State.Playing;
            if (!Owned || !playing) { HideBeams(); return; }

            LaserTuning t = LaserStats.For(damage, cycle, beams, range, Level);

            if (sweepLeft > 0f)
            {
                sweepLeft -= Time.deltaTime;
                angle += (360f / sweepDuration) * Time.deltaTime;
                SweepDamage(t);
                DrawBeams(t);
                if (sweepLeft <= 0f) HideBeams();
                return;
            }

            cooldownLeft -= Time.deltaTime;
            if (cooldownLeft <= 0f)
            {
                sweepLeft = sweepDuration;
                angle = 0f;
                cooldownLeft = t.Cycle;
            }
        }

        void SweepDamage(LaserTuning t)
        {
            float dmg = t.Damage * Time.deltaTime;
            var hits = Physics2D.OverlapCircleAll(transform.position, t.Range, enemyMask);
            foreach (var h in hits)
            {
                Vector2 d = (Vector2)h.transform.position - (Vector2)transform.position;
                float enemyAng = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                for (int b = 0; b < t.Beams; b++)
                {
                    float beamAng = angle + b * (360f / t.Beams);
                    if (Mathf.Abs(Mathf.DeltaAngle(enemyAng, beamAng)) <= wedge)
                    {
                        var target = h.GetComponent<IDamageable>();
                        if (target != null) target.TakeDamage(dmg);
                        break;
                    }
                }
            }
        }

        void DrawBeams(LaserTuning t)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                bool on = i < t.Beams;
                lines[i].enabled = on;
                if (!on) continue;
                float a = (angle + i * (360f / t.Beams)) * Mathf.Deg2Rad;
                Vector3 origin = transform.position;
                lines[i].SetPosition(0, origin);
                lines[i].SetPosition(1, origin + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * t.Range);
            }
        }

        void OnDestroy()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
        }

        void HideBeams()
        {
            if (lines == null) return;
            foreach (var lr in lines) if (lr != null) lr.enabled = false;
        }
    }
}
