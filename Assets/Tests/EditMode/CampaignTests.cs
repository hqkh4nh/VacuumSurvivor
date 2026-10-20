using NUnit.Framework;
using VacuumSurvivor;

public class CampaignTests
{
    [Test]
    public void Starts_at_level_zero()
    {
        Assert.AreEqual(0, new Campaign(3).Index);
    }

    [Test]
    public void Defeating_a_non_final_boss_advances_and_returns_true()
    {
        var c = new Campaign(3);
        Assert.IsTrue(c.BossDefeated());
        Assert.AreEqual(1, c.Index);
    }

    [Test]
    public void Defeating_the_final_boss_returns_false_and_index_stays()
    {
        var c = new Campaign(3);
        c.BossDefeated();
        c.BossDefeated(); // now on level 2, the final one
        Assert.IsTrue(c.IsFinalLevel);
        Assert.IsFalse(c.BossDefeated());
        Assert.AreEqual(2, c.Index);
    }

    [Test]
    public void Index_never_exceeds_last()
    {
        var c = new Campaign(2);
        for (int i = 0; i < 5; i++) c.BossDefeated();
        Assert.AreEqual(1, c.Index);
    }
}
