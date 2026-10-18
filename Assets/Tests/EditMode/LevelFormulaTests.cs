using NUnit.Framework;
using VacuumSurvivor;

public class LevelFormulaTests
{
    [Test] public void Dust_for_level_1_is_9() => Assert.AreEqual(9, LevelFormula.DustForLevel(1));
    [Test] public void Dust_for_level_2_is_13() => Assert.AreEqual(13, LevelFormula.DustForLevel(2));
    [Test] public void Dust_for_level_3_is_17() => Assert.AreEqual(17, LevelFormula.DustForLevel(3));

    [Test]
    public void One_threshold_gains_one_level()
    {
        var p = new LevelProgress();
        Assert.AreEqual(1, p.AddDust(9));
        Assert.AreEqual(2, p.Level);
        Assert.AreEqual(0, p.Dust);
    }

    [Test]
    public void Below_threshold_gains_nothing()
    {
        var p = new LevelProgress();
        Assert.AreEqual(0, p.AddDust(5));
        Assert.AreEqual(1, p.Level);
        Assert.AreEqual(5, p.Dust);
    }

    [Test]
    public void Big_pickup_queues_multiple_levels()
    {
        var p = new LevelProgress();          // L1 needs 9, L2 needs 13 => 22 total
        Assert.AreEqual(2, p.AddDust(22));
        Assert.AreEqual(3, p.Level);
        Assert.AreEqual(0, p.Dust);
    }

    [Test]
    public void Carryover_dust_is_kept()
    {
        var p = new LevelProgress();
        p.AddDust(10);                        // 9 to level, 1 left over
        Assert.AreEqual(2, p.Level);
        Assert.AreEqual(1, p.Dust);
    }
}
