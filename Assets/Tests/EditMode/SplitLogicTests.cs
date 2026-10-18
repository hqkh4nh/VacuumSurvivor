using NUnit.Framework;
using VacuumSurvivor;

public class SplitLogicTests
{
    [Test]
    public void Can_split_yields_split_count() => Assert.AreEqual(2, SplitLogic.ChildCount(true, 2));

    [Test]
    public void Cannot_split_yields_zero() => Assert.AreEqual(0, SplitLogic.ChildCount(false, 2));

    [Test]
    public void Respects_custom_split_count() => Assert.AreEqual(3, SplitLogic.ChildCount(true, 3));
}
