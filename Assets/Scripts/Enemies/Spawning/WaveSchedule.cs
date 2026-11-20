namespace VacuumSurvivor
{
    /// Pure selection of the active wave for a given elapsed time: the latest wave whose start
    /// has passed, or the earliest wave before any has started. Unit-testable in EditMode.
    public static class WaveSchedule
    {
        public static WaveEntry Current(float t, WaveEntry[] entries)
        {
            if (entries == null || entries.Length == 0) return null;

            WaveEntry earliest = entries[0];
            WaveEntry best = null;
            foreach (var e in entries)
            {
                if (e.startTime < earliest.startTime) earliest = e;
                if (e.startTime <= t && (best == null || e.startTime > best.startTime)) best = e;
            }
            return best ?? earliest;
        }
    }
}
