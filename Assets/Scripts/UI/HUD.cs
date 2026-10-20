using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// The in-game overlay: battery bar (colour-coded, blinks when low), dust bar, match clock,
    /// player level and suction level. Builds itself and listens to GameEvents.
    public class HUD : MonoBehaviour
    {
        Image batteryFill;
        Image dustFill;
        Text batteryLabel;
        Text timerLabel;
        Text levelLabel;
        Text weaponLabel;
        GameObject bossBar;
        Image bossFill;
        Text bossLabel;

        float batteryPercent = 1f;
        float bossMax = 1f;
        string room = "Phòng khách";

        void Awake() => Build();

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

        void Build()
        {
            UIFactory.Stretch(gameObject);

            var batteryBg = MakeBar(new Vector2(20f, -20f), new Vector2(320f, 30f), out batteryFill, BatteryColor.Green);
            batteryLabel = UIFactory.Label(batteryBg.transform, "PIN 100%", 16, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(batteryLabel.gameObject);

            MakeBar(new Vector2(20f, -56f), new Vector2(400f, 14f), out dustFill, new Color(0.957f, 0.447f, 0.714f));

            timerLabel = Corner("Màn 1 · 00:00", 22, new Vector2(0.5f, 1f), new Vector2(0f, -20f), TextAnchor.UpperCenter);
            levelLabel = Corner("Cấp 1", 22, new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperRight);
            weaponLabel = Corner("Lực hút Lv1", 18, new Vector2(0f, 0f), new Vector2(20f, 20f), TextAnchor.LowerLeft);

            bossBar = UIFactory.BossBar(transform, out bossFill, out bossLabel);
            bossBar.SetActive(false);
        }

        Image MakeBar(Vector2 pos, Vector2 size, out Image fill, Color fillColor)
        {
            var bg = UIFactory.Box(transform, new Color(0f, 0f, 0f, 0.5f), "BarBg");
            var br = bg.rectTransform;
            br.anchorMin = br.anchorMax = new Vector2(0f, 1f);
            br.pivot = new Vector2(0f, 1f);
            br.sizeDelta = size;
            br.anchoredPosition = pos;

            fill = UIFactory.Box(bg.transform, fillColor, "Fill");
            var fr = fill.rectTransform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(2f, 2f);
            fr.offsetMax = new Vector2(-2f, -2f);
            return bg;
        }

        Text Corner(string text, int size, Vector2 anchor, Vector2 pos, TextAnchor align)
        {
            var t = UIFactory.Label(transform, text, size, align, Color.white);
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = new Vector2(320f, 40f);
            rt.anchoredPosition = pos;
            return t;
        }
    }
}
