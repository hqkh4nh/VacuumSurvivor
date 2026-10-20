using System;

namespace VacuumSurvivor
{
    /// Lightweight pub/sub so UI can react to game state without hard references.
    /// Subscribers MUST unsubscribe in OnDisable; the scene reloads on restart.
    public static class GameEvents
    {
        public static event Action<float, float> BatteryChanged;   // current, max
        public static event Action<int, int> DustChanged;          // current, needed-for-next
        public static event Action<int> PlayerLevelChanged;        // new level
        public static event Action<int> SuctionLevelChanged;       // new weapon level
        public static event Action<string> WeaponsChanged;         // summary of owned weapons + levels
        public static event Action Won;
        public static event Action Lost;
        public static event Action BossPhase;                      // survival over, boss incoming
        public static event Action<float> BossSpawned;             // max health
        public static event Action<float, float> BossHealthChanged; // current, max

        public static void RaiseBattery(float current, float max) => BatteryChanged?.Invoke(current, max);
        public static void RaiseDust(int current, int needed) => DustChanged?.Invoke(current, needed);
        public static void RaisePlayerLevel(int level) => PlayerLevelChanged?.Invoke(level);
        public static void RaiseSuctionLevel(int level) => SuctionLevelChanged?.Invoke(level);
        public static void RaiseWeapons(string summary) => WeaponsChanged?.Invoke(summary);
        public static void RaiseWon() => Won?.Invoke();
        public static void RaiseLost() => Lost?.Invoke();
        public static void RaiseBossPhase() => BossPhase?.Invoke();
        public static void RaiseBossSpawned(float max) => BossSpawned?.Invoke(max);
        public static void RaiseBossHealth(float current, float max) => BossHealthChanged?.Invoke(current, max);
    }
}
