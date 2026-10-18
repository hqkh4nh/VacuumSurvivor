using UnityEngine;

namespace VacuumSurvivor
{
    /// Shared weapon state. All weapons auto-fire; subclasses decide how in their own Update.
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected WeaponData data;

        public WeaponData Data { get => data; set => data = value; }
        public int Level { get; private set; } = 1;
        public int MaxLevel => data != null ? data.maxLevel : 1;

        public void LevelUp()
        {
            if (Level < MaxLevel) Level++;
            OnLevelChanged();
        }

        protected virtual void OnLevelChanged() { }
    }
}
