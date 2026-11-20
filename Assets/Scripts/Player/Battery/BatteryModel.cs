namespace VacuumSurvivor
{
    /// Pure battery math — the game's health stand-in. Drains over time, takes hits, recharges,
    /// and always stays clamped to 0..Max. No Unity types, so it is unit-testable in EditMode.
    public class BatteryModel
    {
        public float Max { get; private set; }
        public float Current { get; private set; }

        public BatteryModel(float max)
        {
            Max = max;
            Current = max;
        }

        public bool IsDead => Current <= 0f;

        public void Tick(float dt, float drainPerSec) => Current = Clamp(Current - drainPerSec * dt);

        public void Damage(float amount) => Current = Clamp(Current - amount);

        public void Charge(float dt, float perSec) => Current = Clamp(Current + perSec * dt);

        public void RechargeFull() => Current = Max;

        public void Add(float amount) => Current = Clamp(Current + amount);

        // Capacity upgrade: raise Max and, when healAdded, top up by the same amount.
        public void AddMax(float amount, bool healAdded)
        {
            Max += amount;
            Current = Clamp(healAdded ? Current + amount : Current);
        }

        float Clamp(float v) => v < 0f ? 0f : (v > Max ? Max : v);
    }
}
