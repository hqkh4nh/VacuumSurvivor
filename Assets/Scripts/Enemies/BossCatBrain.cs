namespace VacuumSurvivor
{
    /// Pure decision/timer kernel for the cat boss: when to shed a hairball, when to telegraph and
    /// land a slap, and when enrage kicks in. No Unity types, so it is unit-testable in EditMode.
    /// The MonoBehaviour owns all movement; this only answers "what should happen this frame".
    public class BossCatBrain
    {
        public enum Phase { Roaming, SlapWindup }

        public struct Decision
        {
            public bool Shed;
            public bool Telegraphing;
            public bool SlapHit;
            public bool Enraged;
            public float SpeedMult;
        }

        readonly float shedInterval;
        readonly float enragedShedInterval;
        readonly float slapCooldown;
        readonly float slapRange;
        readonly float windup;
        readonly float enrageFraction;
        readonly float enragedSpeedMult;

        float shedTimer;
        float slapTimer;
        float windupTimer;

        public Phase Current { get; private set; } = Phase.Roaming;

        public BossCatBrain(float shedInterval = 3f, float enragedShedInterval = 2f,
            float slapCooldown = 6f, float slapRange = 3f, float windup = 0.7f,
            float enrageFraction = 0.3f, float enragedSpeedMult = 1.5f)
        {
            this.shedInterval = shedInterval;
            this.enragedShedInterval = enragedShedInterval;
            this.slapCooldown = slapCooldown;
            this.slapRange = slapRange;
            this.windup = windup;
            this.enrageFraction = enrageFraction;
            this.enragedSpeedMult = enragedSpeedMult;
        }

        public Decision Tick(float dt, float distToPlayer, float hpFraction)
        {
            var d = new Decision
            {
                Enraged = hpFraction < enrageFraction,
            };
            d.SpeedMult = d.Enraged ? enragedSpeedMult : 1f;

            float interval = d.Enraged ? enragedShedInterval : shedInterval;
            shedTimer += dt;
            if (shedTimer >= interval)
            {
                d.Shed = true;
                shedTimer -= interval;
            }

            if (Current == Phase.Roaming)
            {
                slapTimer += dt;
                // Timer is not reset when out of range, so a slap fires as soon as the player
                // steps close once the cooldown has already passed.
                if (slapTimer >= slapCooldown && distToPlayer <= slapRange)
                {
                    Current = Phase.SlapWindup;
                    windupTimer = 0f;
                    slapTimer = 0f;
                }
            }

            if (Current == Phase.SlapWindup)
            {
                d.Telegraphing = true;
                windupTimer += dt;
                if (windupTimer >= windup)
                {
                    d.SlapHit = distToPlayer <= slapRange;
                    Current = Phase.Roaming;
                }
            }

            return d;
        }
    }
}
