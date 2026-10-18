using UnityEngine;

namespace VacuumSurvivor
{
    /// One stretch of a level: how fast trash spawns, the live cap, and which types appear.
    [System.Serializable]
    public class WaveEntry
    {
        public float startTime;
        public float spawnInterval = 1f;
        public int maxAlive = 25;
        public EnemyData[] types;
    }

    /// A level's whole difficulty curve as a list of timed waves. Edited in the Inspector so
    /// balance can change without recompiling.
    [CreateAssetMenu(menuName = "VacuumSurvivor/Wave")]
    public class WaveData : ScriptableObject
    {
        public WaveEntry[] entries;
    }
}
