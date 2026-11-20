using UnityEngine;

namespace VacuumSurvivor
{
    /// Tints the small LED dot on the robot to match the battery level, blinking when it is low.
    public class BatteryLED : MonoBehaviour
    {
        [SerializeField] PlayerStats stats;
        [SerializeField] SpriteRenderer led;
        [SerializeField] float blinkBelow = 0.3f;
        [SerializeField] float blinkSpeed = 6f;

        void Awake()
        {
            if (stats == null) stats = GetComponentInParent<PlayerStats>();
            if (led == null)
            {
                Transform t = transform.Find("LED");
                if (t != null) led = t.GetComponent<SpriteRenderer>();
            }
        }

        void Update()
        {
            if (stats == null || led == null) return;
            float p = stats.Percent;
            Color c = BatteryColor.For(p);
            if (p < blinkBelow)
                c.a = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f));
            led.color = c;
        }
    }
}
