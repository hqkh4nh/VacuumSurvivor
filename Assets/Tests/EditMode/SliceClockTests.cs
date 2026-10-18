using NUnit.Framework;
using VacuumSurvivor;

public class SliceClockTests
{
    [Test] public void Before_duration_is_not_cleared() => Assert.IsFalse(SliceClock.IsCleared(179f, 180f));
    [Test] public void At_duration_is_cleared() => Assert.IsTrue(SliceClock.IsCleared(180f, 180f));
    [Test] public void After_duration_is_cleared() => Assert.IsTrue(SliceClock.IsCleared(200f, 180f));
}
