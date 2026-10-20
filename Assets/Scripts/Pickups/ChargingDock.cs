using UnityEngine;

namespace VacuumSurvivor
{
    /// The "overdrive pad". Standing on it recharges the battery but slows the robot right down —
    /// and doubles the suction's reach and damage, turning a heal into a risky power moment.
    public class ChargingDock : MonoBehaviour
    {
        [SerializeField] float chargePerSecond = 15f;
        [SerializeField] float slowMultiplier = 0.3f;
        [SerializeField] float overdrive = 2f;
        [SerializeField] float lifetime = 20f;
        [SerializeField] float fadeTime = 3f;

        public GameObjectPool Pool { get; set; }

        SpriteRenderer sr;
        Color baseColor;
        GameObject occupant;
        float age;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        void OnEnable()
        {
            age = 0f;
            occupant = null;
            if (sr != null)
            {
                baseColor = sr.color;
                if (baseColor.a < 0.01f) baseColor.a = 0.5f;
                sr.color = baseColor;
            }
        }

        void Update()
        {
            age += Time.deltaTime;

            if (sr != null && age > lifetime - fadeTime)
            {
                float t = Mathf.InverseLerp(lifetime, lifetime - fadeTime, age); // 1 -> 0 near the end
                var c = baseColor; c.a = baseColor.a * t; sr.color = c;
            }

            if (age >= lifetime) ReleaseAndDespawn();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            var stats = other.GetComponent<PlayerStats>();
            if (stats == null) return;
            occupant = other.gameObject;
            stats.Charge(chargePerSecond);
            SetBuffs(occupant, slowMultiplier, overdrive);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject != occupant) return;
            // Overlapping docks would otherwise flicker the buff off for a frame: keep it while the
            // robot is still standing on another dock.
            if (!StillOnAnotherDock(other.transform.position)) SetBuffs(occupant, 1f, 1f);
            occupant = null;
        }

        bool StillOnAnotherDock(Vector2 playerPos)
        {
            var hits = Physics2D.OverlapCircleAll(playerPos, 0.1f, LayerMask.GetMask("Pickup"));
            foreach (var h in hits)
            {
                var dock = h.GetComponent<ChargingDock>();
                if (dock != null && dock != this) return true;
            }
            return false;
        }

        void ReleaseAndDespawn()
        {
            if (occupant != null) { SetBuffs(occupant, 1f, 1f); occupant = null; }
            if (Pool != null) Pool.Return(gameObject);
            else gameObject.SetActive(false);
        }

        static void SetBuffs(GameObject playerGo, float speedMultiplier, float overdrive)
        {
            var pc = playerGo.GetComponent<PlayerController>();
            if (pc != null) pc.SpeedMultiplier = speedMultiplier;
            var sw = playerGo.GetComponentInChildren<SuctionWeapon>();
            if (sw != null) sw.Overdrive = overdrive;
        }
    }
}
