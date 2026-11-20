namespace VacuumSurvivor
{
    /// Pure hit-point tracking for trash. No Unity types, so it is unit-testable in EditMode.
    public class Health
    {
        public float Max { get; private set; }
        public float Current { get; private set; }

        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        public bool IsDead => Current <= 0f;

        public void Damage(float amount)
        {
            Current -= amount;
            if (Current < 0f) Current = 0f;
        }
    }
}
