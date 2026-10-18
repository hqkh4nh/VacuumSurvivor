namespace VacuumSurvivor
{
    /// Pure dust-to-level bookkeeping, including multi-level gains from one big pickup. No Unity
    /// types, so it is unit-testable in EditMode.
    public class LevelProgress
    {
        public int Level { get; private set; } = 1;
        public int Dust { get; private set; }
        public int DustForNext => LevelFormula.DustForLevel(Level);

        /// Adds dust and returns how many levels were gained (0, 1, or several).
        public int AddDust(int amount)
        {
            Dust += amount;
            int gained = 0;
            while (Dust >= DustForNext)
            {
                Dust -= DustForNext;
                Level++;
                gained++;
            }
            return gained;
        }
    }
}
