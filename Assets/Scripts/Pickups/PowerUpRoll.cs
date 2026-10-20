namespace VacuumSurvivor
{
    /// Pure roll for floor power-up drops: whether a kill drops one, and which kind. Kept Unity-free
    /// so the drop odds are unit-testable; the spawner feeds it a random value per kill.
    public static class PowerUpRoll
    {
        public static bool ShouldDrop(double roll01, float chance) => roll01 < chance;

        public static PowerUpKind Pick(double roll01)
        {
            if (roll01 < 1.0 / 3.0) return PowerUpKind.Battery;
            if (roll01 < 2.0 / 3.0) return PowerUpKind.Magnet;
            return PowerUpKind.Bomb;
        }
    }
}
