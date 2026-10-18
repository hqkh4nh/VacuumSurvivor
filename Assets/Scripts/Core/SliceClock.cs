namespace VacuumSurvivor
{
    /// Pure check for whether the slice's survival time has been reached.
    public static class SliceClock
    {
        public static bool IsCleared(float elapsed, float duration) => elapsed >= duration;
    }
}
