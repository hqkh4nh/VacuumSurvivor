using NUnit.Framework;
using VacuumSurvivor;
using UnityEngine;

public class RunStatsTests
{
    [Test]
    public void DustCollectionReportsFullPickupEvenWhenItSpansLevels()
    {
        var robot = new GameObject("Robot");
        var stats = robot.AddComponent<PlayerStats>();
        int collected = 0;
        void Record(int amount) => collected += amount;

        GameEvents.DustCollected += Record;
        try
        {
            stats.AddDust(100);
            Assert.Greater(stats.Level, 2);
            Assert.AreEqual(100, collected);
        }
        finally
        {
            GameEvents.DustCollected -= Record;
            Object.DestroyImmediate(robot);
        }
    }

    [Test]
    public void Score_without_win_bonus()
    {
        var s = new RunStats { dust = 100, kills = 50, seconds = 300 };
        Assert.AreEqual(100 * 10 + 50 * 5 + 300 * 2, s.Score(false));
    }

    [Test]
    public void Score_with_win_bonus_adds_5000()
    {
        var s = new RunStats { dust = 100, kills = 50, seconds = 300 };
        Assert.AreEqual(s.Score(false) + 5000, s.Score(true));
    }

    [Test]
    public void All_zero_loss_scores_zero()
    {
        Assert.AreEqual(0, new RunStats().Score(false));
    }
}
