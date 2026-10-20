namespace VacuumSurvivor
{
    /// Tallies of a single run and the end-of-run score. Pure, so it is unit-testable in EditMode.
    public struct RunStats
    {
        public int dust;
        public int kills;
        public int seconds;

        /// GDD §1.8.3: bụi × 10 + rác × 5 + giây × 2, plus a 5000 bonus for winning.
        public int Score(bool won) => dust * 10 + kills * 5 + seconds * 2 + (won ? 5000 : 0);
    }
}
