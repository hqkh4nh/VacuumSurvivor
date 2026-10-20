namespace VacuumSurvivor
{
    /// Tracks which level the run is on and decides, when a boss dies, whether to advance to the
    /// next level or end the whole game in a win. Pure, so it is unit-testable in EditMode.
    public class Campaign
    {
        readonly int levelCount;

        public int Index { get; private set; }

        public Campaign(int levelCount)
        {
            this.levelCount = levelCount < 1 ? 1 : levelCount;
        }

        public bool IsFinalLevel => Index >= levelCount - 1;

        /// Called when the current level's boss dies. Advances to the next level and returns true
        /// when there is one; on the final level it stays put and returns false (the run is won).
        public bool BossDefeated()
        {
            if (IsFinalLevel) return false;
            Index++;
            return true;
        }
    }
}
