using UnityEngine;

namespace VacuumSurvivor
{
    /// Cục tóc rối: crawls like a Chaser but, on death, bursts into smaller trash. The children are
    /// plain Chasers, so "the little ones don't split again" is guaranteed by their type.
    public class SplitterEnemy : ChaserEnemy
    {
        [SerializeField] bool canSplit = true;
        [SerializeField] GameObject childPrefab;

        public bool CanSplit { get => canSplit; set => canSplit = value; }
        public GameObject ChildPrefab { get => childPrefab; set => childPrefab = value; }

        protected override void Die()
        {
            int n = SplitLogic.ChildCount(canSplit, data.splitCount);
            for (int i = 0; i < n; i++)
                SpawnChild((Vector2)transform.position + Random.insideUnitCircle * 0.3f);
            base.Die();
        }

        void SpawnChild(Vector2 pos)
        {
            if (SpawnRequest != null && data.splitInto != null) SpawnRequest(data.splitInto, pos);
            else if (childPrefab != null) Instantiate(childPrefab, pos, Quaternion.identity);
        }
    }
}
