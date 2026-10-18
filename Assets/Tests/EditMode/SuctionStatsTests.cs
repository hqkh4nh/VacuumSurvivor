using NUnit.Framework;
using VacuumSurvivor;

public class SuctionStatsTests
{
    static readonly float[] Dps = { 4f, 5.5f, 7f, 8.5f, 10f };
    static readonly float[] Radius = { 1.5f, 1.8f, 2.1f, 2.45f, 2.8f };

    [Test]
    public void Level_one_uses_first_entry()
    {
        var t = SuctionStats.For(Dps, Radius, 1, 1f);
        Assert.AreEqual(4f, t.Dps, 1e-4f);
        Assert.AreEqual(1.5f, t.Radius, 1e-4f);
    }

    [Test]
    public void Level_five_uses_last_entry()
    {
        var t = SuctionStats.For(Dps, Radius, 5, 1f);
        Assert.AreEqual(10f, t.Dps, 1e-4f);
        Assert.AreEqual(2.8f, t.Radius, 1e-4f);
    }

    [Test]
    public void Overdrive_doubles_both()
    {
        var t = SuctionStats.For(Dps, Radius, 1, 2f);
        Assert.AreEqual(8f, t.Dps, 1e-4f);
        Assert.AreEqual(3f, t.Radius, 1e-4f);
    }

    [Test]
    public void Level_above_max_clamps_to_last()
    {
        var t = SuctionStats.For(Dps, Radius, 99, 1f);
        Assert.AreEqual(10f, t.Dps, 1e-4f);
    }
}
