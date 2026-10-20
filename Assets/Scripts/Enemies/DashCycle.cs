namespace VacuumSurvivor
{
    /// Pure timer kernel for Tất lẻ: aim in place (telegraphing), lunge, recover, repeat. No Unity
    /// types, so it is unit-testable in EditMode. The MonoBehaviour owns movement and reads the
    /// locked dash direction on the frame StartedDash flips true.
    public class DashCycle
    {
        public enum Phase { Aiming, Dashing, Recovering }

        public struct Decision
        {
            public Phase Phase;
            public bool StartedDash;
            public bool Telegraphing;
        }

        readonly float aimTime;
        readonly float dashDuration;
        readonly float recoverTime;
        float timer;

        public Phase Current { get; private set; } = Phase.Aiming;

        public DashCycle(float aimTime = 1f, float dashDuration = 0.5f, float recoverTime = 0.6f)
        {
            this.aimTime = aimTime;
            this.dashDuration = dashDuration;
            this.recoverTime = recoverTime;
        }

        public Decision Tick(float dt)
        {
            timer += dt;
            bool started = false;

            switch (Current)
            {
                case Phase.Aiming:
                    if (timer >= aimTime) { Current = Phase.Dashing; timer -= aimTime; started = true; }
                    break;
                case Phase.Dashing:
                    if (timer >= dashDuration) { Current = Phase.Recovering; timer -= dashDuration; }
                    break;
                case Phase.Recovering:
                    if (timer >= recoverTime) { Current = Phase.Aiming; timer -= recoverTime; }
                    break;
            }

            return new Decision
            {
                Phase = Current,
                StartedDash = started,
                Telegraphing = Current == Phase.Aiming,
            };
        }
    }
}
