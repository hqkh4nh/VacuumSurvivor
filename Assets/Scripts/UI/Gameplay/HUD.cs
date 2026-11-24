using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// Displays the run state published by GameEvents on the authored Canvas.
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

        static readonly Color Mint = new Color(0.59f, 0.98f, 0.83f);
        static readonly Color BatteryWarning = new Color(1f, 0.68f, 0.61f);
        static readonly Color DustColor = new Color(0.45f, 0.82f, 0.98f);

        float batteryPercent = 1f;
        float dustTarget;
        float dustDisplayed;
        float bossMax = 1f;
        float bossTarget = 1f;
        float bossTrail = 1f;
        Image bossDamageTrail;
        bool batteryHit;
        float batteryHitUntil;
        string room = "Phòng khách";
        string clockRoom;
        int clockSecond = -1;
        bool clockPaused;
        int batteryNumber = -1;
        bool batteryLow;

        void Awake()
        {
            SetBar(dustFill, 0f);
            dustFill.color = DustColor;
            bossBar.SetActive(false);
            bossDamageTrail = Instantiate(bossFill, bossFill.transform.parent, false);
            bossDamageTrail.name = "Recent Damage";
            bossDamageTrail.transform.SetSiblingIndex(bossFill.transform.GetSiblingIndex());
            bossDamageTrail.color = new Color(1f, 0.76f, 0.48f);
        }

        void OnEnable()
        {
            GameEvents.BatteryChanged += OnBattery;
            GameEvents.DustChanged += OnDust;
            GameEvents.PlayerLevelChanged += OnLevel;
            GameEvents.WeaponsChanged += OnWeapons;
            GameEvents.BossSpawned += OnBossSpawned;
            GameEvents.BossHealthChanged += OnBossHealth;
            GameEvents.LevelChanged += OnLevelChanged;
            GameEvents.PlayerHurt += OnPlayerHurt;
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
            GameEvents.PlayerHurt -= OnPlayerHurt;
        }

        void Start()
        {
            if (GameManager.Instance != null)
            {
                var plate = transform.Find("RoomPlate");
                var frame = plate != null ? plate.GetComponent<Image>()?.sprite : null;
                gameObject.AddComponent<GameplayOverlay>().Configure(timerLabel.font, frame);
            }
            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player == null) return;
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                OnBattery(stats.Battery, stats.MaxBattery);
                OnDust(stats.Dust, stats.DustForNext);
                OnLevel(stats.Level);
            }

            var weapons = player.GetComponent<WeaponManager>();
            if (weapons != null) OnWeapons(weapons.Summary());
        }

        void Update()
        {
            UpdateClock();

            bool low = batteryPercent < 0.3f;
            batteryHit = batteryHit && Time.unscaledTime < batteryHitUntil;
            if (low || batteryHit)
            {
                var color = batteryHit ? BatteryWarning : BatteryColor.Red;
                color.a = low ? 0.82f + 0.18f * Mathf.Cos(Time.unscaledTime * 5f) : 1f;
                batteryFill.color = color;
            }
            else if (batteryFill.color != BatteryColor.For(batteryPercent))
            {
                batteryFill.color = BatteryColor.For(batteryPercent);
            }

            float step = Time.unscaledDeltaTime;
            if (dustDisplayed != dustTarget)
            {
                dustDisplayed = Mathf.MoveTowards(dustDisplayed, dustTarget, step * 3.5f);
                SetBar(dustFill, dustDisplayed);
            }

            if (bossBar.activeSelf && bossTrail != bossTarget)
            {
                bossTrail = Mathf.MoveTowards(bossTrail, bossTarget, step * 0.9f);
                SetBar(bossDamageTrail, bossTrail);
            }
        }

        void UpdateClock()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            int second = Mathf.Max(0, (int)manager.Elapsed);
            bool paused = manager.Current == GameManager.State.Paused;
            if (second == clockSecond && paused == clockPaused && room == clockRoom) return;

            clockSecond = second;
            clockPaused = paused;
            clockRoom = room;
            timerLabel.text = paused ? "TẠM DỪNG · ESC để tiếp tục" : $"{room} · {second / 60:00}:{second % 60:00}";
            timerLabel.color = paused ? BatteryColor.Yellow : Mint;
        }

        void OnBattery(float current, float max)
        {
            batteryPercent = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            SetBar(batteryFill, batteryPercent);
            int number = Mathf.CeilToInt(batteryPercent * 100f);
            bool low = batteryPercent < 0.3f;
            if (number == batteryNumber && low == batteryLow) return;

            batteryNumber = number;
            batteryLow = low;
            batteryLabel.color = low ? BatteryWarning : Color.white;
            batteryLabel.text = low ? $"PIN YẾU · {number}%" : $"PIN {number}%";
        }

        void OnPlayerHurt()
        {
            batteryHit = true;
            batteryHitUntil = Time.unscaledTime + 0.17f;
        }

        void OnDust(int current, int needed)
        {
            dustTarget = needed > 0 ? Mathf.Clamp01((float)current / needed) : 0f;
        }

        void OnLevel(int level)
        {
            levelLabel.text = $"Cấp {level}";
            dustDisplayed = dustTarget;
            SetBar(dustFill, dustDisplayed);
        }

        void OnWeapons(string summary) => weaponLabel.text = summary.Replace("  ", "  ·  ");

        void OnLevelChanged(int index, string name)
        {
            room = name;
            bossLabel.text = name == "Nhà bếp" ? "MÁY HÚT BỤI CŨ" : "CON MÈO";
        }

        void OnBossSpawned(float max)
        {
            bossMax = max > 0f ? max : 1f;
            bossTarget = bossTrail = 1f;
            SetBar(bossFill, bossTarget);
            SetBar(bossDamageTrail, bossTrail);
            bossBar.SetActive(true);
            PanelEntrance.Play(bossBar);
        }

        void OnBossHealth(float current, float max)
        {
            if (max > 0f) bossMax = max;
            bossTarget = Mathf.Clamp01(current / bossMax);
            SetBar(bossFill, bossTarget);
            if (bossTarget > bossTrail)
            {
                bossTrail = bossTarget;
                SetBar(bossDamageTrail, bossTrail);
            }
            if (current <= 0f) bossBar.SetActive(false);
        }

        static void SetBar(Image fill, float progress)
        {
            var rect = fill.rectTransform;
            var anchors = rect.anchorMax;
            if (anchors.x != progress)
            {
                anchors.x = progress;
                rect.anchorMax = anchors;
            }
            // The inset fill has a negative size delta. Hide it below its minimum usable width.
            fill.enabled = progress * ((RectTransform)rect.parent).rect.width + rect.sizeDelta.x > 0f;
        }
    }
}
