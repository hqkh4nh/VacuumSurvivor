namespace VacuumSurvivor
{
    /// Dust needed to advance from the given level to the next: 5 + level*4 (GDD 1.4.5).
    public static class LevelFormula
    {
        public static int DustForLevel(int level) => 5 + level * 4;
    }
}
