using UnityEngine;

namespace VacuumSurvivor
{
    /// Per-type trash stats. Keeping these in a ScriptableObject lets the whole roster be balanced
    /// in the Inspector without recompiling, and lets several trash types share one behaviour class.
    [CreateAssetMenu(menuName = "VacuumSurvivor/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName = "Enemy";
        public float maxHealth = 10f;
        public float moveSpeed = 1.8f;
        public float contactDamage = 5f;
        public int dustValue = 1;
        public Color bodyColor = new Color(0.545f, 0.545f, 0.62f); // #8B8B9E
        public int bodyPixels = 44;

        [Header("Swarm (0 = spawn singly)")]
        public int swarmMin;
        public int swarmMax;

        [Header("Splitter only")]
        public bool splitsOnDeath;
        public EnemyData splitInto;
        public int splitCount = 2;

        [Header("Dasher only")]
        public float aimTime = 1f;
        public float dashSpeed = 9f;
        public float dashDuration = 0.5f;
        public float recoverTime = 0.6f;

        [Header("Hazard only")]
        public float lifetime;        // 0 = permanent
        public bool indestructible;

        [Header("Snake only")]
        public float rootDuration = 1f;
        public float weaveAmp = 0.6f;
        public float weaveFreq = 6f;
    }
}
