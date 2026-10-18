using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VacuumSurvivor;

public class UpgradeRollTests
{
    static UpgradeData Make(int maxLevel = 5)
    {
        var u = ScriptableObject.CreateInstance<UpgradeData>();
        u.maxLevel = maxLevel;
        return u;
    }

    [Test]
    public void Picks_the_requested_count()
    {
        var all = new List<UpgradeData> { Make(), Make(), Make(), Make(), Make() };
        var r = UpgradeRoll.Pick(all, _ => 0, 3, new System.Random(1));
        Assert.AreEqual(3, r.Count);
    }

    [Test]
    public void Picks_are_distinct()
    {
        var all = new List<UpgradeData> { Make(), Make(), Make(), Make(), Make() };
        var r = UpgradeRoll.Pick(all, _ => 0, 3, new System.Random(2));
        CollectionAssert.AllItemsAreUnique(r);
    }

    [Test]
    public void Skips_maxed_upgrades()
    {
        var a = Make(); var b = Make(); var c = Make();
        var levels = new Dictionary<UpgradeData, int> { { a, 5 }, { b, 5 } };
        var r = UpgradeRoll.Pick(new List<UpgradeData> { a, b, c },
            u => levels.TryGetValue(u, out int l) ? l : 0, 3, new System.Random(3));
        Assert.AreEqual(1, r.Count);
        Assert.AreSame(c, r[0]);
    }

    [Test]
    public void Returns_all_when_fewer_than_requested()
    {
        var all = new List<UpgradeData> { Make(), Make() };
        var r = UpgradeRoll.Pick(all, _ => 0, 3, new System.Random(4));
        Assert.AreEqual(2, r.Count);
    }
}
