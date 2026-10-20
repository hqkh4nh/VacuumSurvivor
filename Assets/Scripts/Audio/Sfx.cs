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
        readonly Dictionary<AudioClip, float> nextAllowed = new Dictionary<AudioClip, float>();

        void Awake()
        {
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.volume = 0.5f;

            music = gameObject.AddComponent<AudioSource>();
            music.clip = SoundFactory.MusicLoop();
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
        }

        void Start() => music.Play();

        void OnEnable()
        {
            GameEvents.DustChanged += OnDust;
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
            GameEvents.DustChanged -= OnDust;
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

        void OnDust(int current, int needed) => Play(pickup);
        void OnLevel(int level) => Play(levelUp);
        void OnHurt() => Play(hit);
        void OnShot() => Play(shoot);
        void OnPowerUp(PowerUpKind kind) => Play(powerUp);
        void OnBoss() => Play(boss);
        void OnWon() => Play(win);
        void OnLost() => Play(lose);

        void OnRoom(int index, string name)
        {
            if (music != null) music.pitch = RoomPitch[Mathf.Clamp(index, 0, RoomPitch.Length - 1)];
        }
    }
}
