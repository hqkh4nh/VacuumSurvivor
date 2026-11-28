using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Plays the generated sound effects and background music, wiring itself to GameEvents. One
    /// instance per game scene; it builds every clip once in Awake. A per-clip gap throttles events
    /// that can fire in bursts (dust pickups) so they don't buzz.
    public class Sfx : MonoBehaviour
    {
        const float MinGap = 0.05f;
        static readonly float[] RoomPitch = { 1f, 1.08f, 0.94f };

        AudioSource sfx;
        AudioSource music;
        AudioClip shoot, pickup, hit, levelUp, boss, win, lose, powerUp;
        AudioClip lowBattery, dockEngaged, roomMusic, bossMusic;
        bool lowBatteryWarned;
        readonly Dictionary<AudioClip, float> nextAllowed = new Dictionary<AudioClip, float>();

        void Awake()
        {
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.volume = 0.5f;

            music = gameObject.AddComponent<AudioSource>();
            roomMusic = SoundFactory.MusicLoop();
            bossMusic = SoundFactory.BossMusicLoop();
            music.clip = roomMusic;
            music.loop = true;
            music.volume = 0.25f;
            music.playOnAwake = false;

            shoot = SoundFactory.Shoot();
            pickup = SoundFactory.Pickup();
            hit = SoundFactory.Hit();
            levelUp = SoundFactory.LevelUp();
            boss = SoundFactory.Boss();
            win = SoundFactory.Win();
            lose = SoundFactory.Lose();
            powerUp = SoundFactory.PowerUp();
            lowBattery = SoundFactory.LowBattery();
            dockEngaged = SoundFactory.DockEngaged();
        }

        void Start() => music.Play();

        void OnEnable()
        {
            GameEvents.DustCollected += OnDust;
            GameEvents.BatteryChanged += OnBattery;
            GameEvents.DockEngaged += OnDockEngaged;
            GameEvents.PlayerLevelChanged += OnLevel;
            GameEvents.PlayerHurt += OnHurt;
            GameEvents.Shot += OnShot;
            GameEvents.PowerUpCollected += OnPowerUp;
            GameEvents.BossPhase += OnBoss;
            GameEvents.Won += OnWon;
            GameEvents.Lost += OnLost;
            GameEvents.LevelChanged += OnRoom;
        }

        void OnDisable()
        {
            GameEvents.DustCollected -= OnDust;
            GameEvents.BatteryChanged -= OnBattery;
            GameEvents.DockEngaged -= OnDockEngaged;
            GameEvents.PlayerLevelChanged -= OnLevel;
            GameEvents.PlayerHurt -= OnHurt;
            GameEvents.Shot -= OnShot;
            GameEvents.PowerUpCollected -= OnPowerUp;
            GameEvents.BossPhase -= OnBoss;
            GameEvents.Won -= OnWon;
            GameEvents.Lost -= OnLost;
            GameEvents.LevelChanged -= OnRoom;
        }

        void Play(AudioClip clip)
        {
            if (clip == null || sfx == null) return;
            if (nextAllowed.TryGetValue(clip, out float t) && Time.unscaledTime < t) return;
            nextAllowed[clip] = Time.unscaledTime + MinGap;
            sfx.PlayOneShot(clip);
        }

        void OnDust(int amount) => Play(pickup);
        void OnDockEngaged() => Play(dockEngaged);
        void OnBattery(float current, float max)
        {
            if (max <= 0f || current <= 0f) return;
            float fraction = current / max;
            // A small recovery margin prevents repeat alerts near the warning threshold.
            if (fraction >= 0.35f) lowBatteryWarned = false;
            if (fraction >= 0.3f || lowBatteryWarned) return;
            lowBatteryWarned = true;
            Play(lowBattery);
        }
        void OnLevel(int level) => Play(levelUp);
        void OnHurt() => Play(hit);
        void OnShot() => Play(shoot);
        void OnPowerUp(PowerUpKind kind) => Play(powerUp);
        void OnBoss()
        {
            Play(boss);
            music.pitch = 1f;
            music.clip = bossMusic;
            music.Play();
        }

        void OnWon()
        {
            music.Stop();
            Play(win);
        }

        void OnLost()
        {
            music.Stop();
            Play(lose);
        }

        void OnRoom(int index, string name)
        {
            music.pitch = RoomPitch[Mathf.Clamp(index, 0, RoomPitch.Length - 1)];
            if (music.clip == roomMusic) return;
            music.clip = roomMusic;
            music.Play();
        }
    }
}
