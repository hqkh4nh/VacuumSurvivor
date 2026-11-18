using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// The in-game overlay: battery bar (colour-coded, blinks when low), dust bar, match clock,
    /// player level and weapons. Listens to GameEvents and updates the authored Canvas.
    public class HUD : MonoBehaviour
    {
        [SerializeField] Image batteryFill;
        [SerializeField] Image dustFill;
        [SerializeField] Text batteryLabel;
        [SerializeField] Text timerLabel;
        [SerializeField] Text levelLabel;
        [SerializeField] Text weaponLabel;
        [SerializeField] GameObject bossBar;
        [SerializeField] Image bossFill;
        [SerializeField] Text bossLabel;

        float batteryPercent = 1f;
        float bossMax = 1f;
        string room = "Phòng khách";

        void Awake() => bossBar.SetActive(false);

        void OnEnable()
        {
            GameEvents.BatteryChanged += OnBattery;
            GameEvents.DustChanged += OnDust;
            GameEvents.PlayerLevelChanged += OnLevel;
            GameEvents.WeaponsChanged += OnWeapons;
            GameEvents.BossSpawned += OnBossSpawned;
            GameEvents.BossHealthChanged += OnBossHealth;
            GameEvents.LevelChanged += OnLevelChanged;
        }

        void OnDisable()
        {
            GameEvents.BatteryChanged -= OnBattery;
            GameEvents.DustChanged -= OnDust;
            GameEvents.PlayerLevelChanged -= OnLevel;
            GameEvents.WeaponsChanged -= OnWeapons;
            GameEvents.BossSpawned -= OnBossSpawned;
            GameEvents.BossHealthChanged -= OnBossHealth;
            GameEvents.LevelChanged -= OnLevelChanged;
        }

        void Start()
        {
            // Pick up state that may have been published before this HUD subscribed.
            var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (p == null) return;
            var stats = p.GetComponent<PlayerStats>();
            if (stats != null) { OnBattery(stats.Battery, stats.MaxBattery); OnDust(stats.Dust, stats.DustForNext); OnLevel(stats.Level); }
            var wm = p.GetComponent<WeaponManager>();
            if (wm != null) OnWeapons(wm.Summary());
        }

        void Update()
        {
            if (GameManager.Instance != null)
            {
                float t = GameManager.Instance.Elapsed;
                timerLabel.text = $"{room} · {(int)t / 60:00}:{(int)t % 60:00}";
            }

            if (batteryPercent < 0.3f)
            {
                float a = Mathf.Lerp(0.4f, 1f, Mathf.PingPong(Time.unscaledTime * 6f, 1f));
                var c = batteryFill.color; c.a = a; batteryFill.color = c;
            }
        }

        void OnBattery(float current, float max)
        {
            batteryPercent = max > 0f ? current / max : 0f;
            SetBar(batteryFill, batteryPercent);
            var c = BatteryColor.For(batteryPercent); c.a = 1f; batteryFill.color = c;
            batteryLabel.text = $"PIN {Mathf.CeilToInt(batteryPercent * 100f)}%";
        }

        void OnDust(int current, int needed) => SetBar(dustFill, needed > 0 ? (float)current / needed : 0f);
        void OnLevel(int level) => levelLabel.text = $"Cấp {level}";
        void OnWeapons(string summary) => weaponLabel.text = summary;

        void OnLevelChanged(int index, string name)
        {
            room = name;
            // The kitchen is the final room, where the old handheld vacuum shows up instead of the cat.
            bossLabel.text = name == "Nhà bếp" ? "MÁY HÚT BỤI CŨ" : "CON MÈO";
        }

        void OnBossSpawned(float max)
        {
            bossMax = max > 0f ? max : 1f;
            bossBar.SetActive(true);
            SetBar(bossFill, 1f);
        }

        void OnBossHealth(float current, float max)
        {
            if (max > 0f) bossMax = max;
            SetBar(bossFill, bossMax > 0f ? current / bossMax : 0f);
            if (current <= 0f) bossBar.SetActive(false);
        }

        static void SetBar(Image fill, float t)
        {
            var rt = fill.rectTransform;
            var a = rt.anchorMax;
            a.x = Mathf.Clamp01(t);
            rt.anchorMax = a;
        }

    }
}
