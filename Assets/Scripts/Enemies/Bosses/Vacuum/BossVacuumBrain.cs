namespace VacuumSurvivor
{
    /// Pure decision/timer kernel for the final boss (Máy hút bụi cũ): the fixed attack cycle
    /// Hút → Thổi → Lao, each separated by a rest, with a telegraph before the dash. No Unity
    /// types, so it is unit-testable in EditMode. The MonoBehaviour applies the decision to the
    /// world (pulling the player, spawning the blow ring, driving the dash).
    public class BossVacuumBrain
    {
        public enum Phase { Suck, RestAfterSuck, Blow, RestAfterBlow, DashTelegraph, Dash, RestAfterDash }

        public struct Decision
        {
            public Phase Phase;
            public bool Pulling;
            public bool BlowBurst;
            public bool DashTelegraphing;
            public bool DashActive;
        }

        readonly float suckDuration;
        readonly float restDuration;
        readonly float telegraphDuration;
        readonly float dashDuration;

        Phase phase = Phase.Suck;
        float timer;

        public Phase Current => phase;

        public BossVacuumBrain(float suckDuration = 3f, float restDuration = 1.5f,
            float telegraphDuration = 1f, float dashDuration = 0.6f)
        {
            this.suckDuration = suckDuration;
            this.restDuration = restDuration;
            this.telegraphDuration = telegraphDuration;
            this.dashDuration = dashDuration;
        }

        public Decision Tick(float dt)
        {
            timer += dt;
            var d = new Decision { Phase = phase };
            switch (phase)
            {
                case Phase.Suck:
                    d.Pulling = true;
                    if (timer >= suckDuration) Advance(Phase.RestAfterSuck);
                    break;
                case Phase.RestAfterSuck:
                    if (timer >= restDuration) Advance(Phase.Blow);
                    break;
                case Phase.Blow:
                    // Single-tick attack: emit the ring once, then straight into the rest.
                    d.BlowBurst = true;
                    Advance(Phase.RestAfterBlow);
                    break;
                case Phase.RestAfterBlow:
                    if (timer >= restDuration) Advance(Phase.DashTelegraph);
                    break;
                case Phase.DashTelegraph:
                    d.DashTelegraphing = true;
                    if (timer >= telegraphDuration) Advance(Phase.Dash);
                    break;
                case Phase.Dash:
                    d.DashActive = true;
                    if (timer >= dashDuration) Advance(Phase.RestAfterDash);
                    break;
                case Phase.RestAfterDash:
                    if (timer >= restDuration) Advance(Phase.Suck);
                    break;
            }
            return d;
        }

        void Advance(Phase next)
        {
            phase = next;
            timer = 0f;
        }
    }
}
