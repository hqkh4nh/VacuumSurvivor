using NUnit.Framework;
using VacuumSurvivor;

public class OrbitStatsTests
{
    static readonly int[] Count = { 1, 2, 3, 3, 4 };
    static readonly float[] Dps = { 6f, 8f, 10f, 12f, 14f };
    static readonly float[] Radius = { 1.8f, 1.9f, 2.0f, 2.1f, 2.2f };
    static readonly float[] RotSpeed = { 120f, 140f, 160f, 180f, 200f };

    [Test]
    public void Level_one_uses_first_entry()
    {
        var t = OrbitStats.For(Count, Dps, Radius, RotSpeed, 1);
        Assert.AreEqual(1, t.Count);
        Assert.AreEqual(6f, t.Dps, 1e-4f);
        Assert.AreEqual(1.8f, t.Radius, 1e-4f);
        Assert.AreEqual(120f, t.RotSpeed, 1e-4f);
    }

    [Test]
    public void Level_five_uses_last_entry()
    {
        var t = OrbitStats.For(Count, Dps, Radius, RotSpeed, 5);
        Assert.AreEqual(4, t.Count);
        Assert.AreEqual(14f, t.Dps, 1e-4f);
        Assert.AreEqual(2.2f, t.Radius, 1e-4f);
        Assert.AreEqual(200f, t.RotSpeed, 1e-4f);
    }

    [Test]
    public void Level_above_max_clamps_to_last()
    {
        var t = OrbitStats.For(Count, Dps, Radius, RotSpeed, 99);
        Assert.AreEqual(4, t.Count);
        Assert.AreEqual(14f, t.Dps, 1e-4f);
    }

    [Test]
    public void Broom_count_is_non_decreasing()
    {
        int prev = 0;
        for (int lvl = 1; lvl <= Count.Length; lvl++)
        {
            int c = OrbitStats.For(Count, Dps, Radius, RotSpeed, lvl).Count;
            Assert.GreaterOrEqual(c, prev);
            prev = c;
        }
    }
}
