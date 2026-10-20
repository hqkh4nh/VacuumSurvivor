using NUnit.Framework;
using VacuumSurvivor;

public class ProjectileStatsTests
{
    static readonly float[] Damage = { 12f, 14f, 16f, 16f, 18f };
    static readonly float[] Cooldown = { 1.2f, 1.05f, 0.9f, 0.8f, 0.7f };
    static readonly int[] Shots = { 1, 1, 2, 2, 3 };
    static readonly float[] Range = { 6f, 6.5f, 7f, 7.5f, 8f };

    [Test]
    public void Level_one_uses_first_entry()
    {
        var t = ProjectileStats.For(Damage, Cooldown, Shots, Range, 9f, 1);
        Assert.AreEqual(12f, t.Damage, 1e-4f);
        Assert.AreEqual(1.2f, t.Cooldown, 1e-4f);
        Assert.AreEqual(1, t.Shots);
        Assert.AreEqual(6f, t.Range, 1e-4f);
        Assert.AreEqual(9f, t.Speed, 1e-4f);
    }

    [Test]
    public void Level_five_uses_last_entry()
    {
        var t = ProjectileStats.For(Damage, Cooldown, Shots, Range, 9f, 5);
        Assert.AreEqual(18f, t.Damage, 1e-4f);
        Assert.AreEqual(0.7f, t.Cooldown, 1e-4f);
        Assert.AreEqual(3, t.Shots);
        Assert.AreEqual(8f, t.Range, 1e-4f);
    }

    [Test]
    public void Level_above_max_clamps_to_last()
    {
        var t = ProjectileStats.For(Damage, Cooldown, Shots, Range, 9f, 99);
        Assert.AreEqual(18f, t.Damage, 1e-4f);
        Assert.AreEqual(3, t.Shots);
    }

    [Test]
    public void Shot_count_is_non_decreasing()
    {
        int prev = 0;
        for (int lvl = 1; lvl <= Shots.Length; lvl++)
        {
            int s = ProjectileStats.For(Damage, Cooldown, Shots, Range, 9f, lvl).Shots;
            Assert.GreaterOrEqual(s, prev);
            prev = s;
        }
    }
}
