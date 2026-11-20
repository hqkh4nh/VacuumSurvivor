namespace VacuumSurvivor
{
    /// Pure rule for how many children a splitting enemy drops. Isolated so it is unit-testable.
    public static class SplitLogic
    {
        public static int ChildCount(bool canSplit, int splitCount) => canSplit ? splitCount : 0;
    }
}
