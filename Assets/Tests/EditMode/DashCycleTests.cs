using NUnit.Framework;
using VacuumSurvivor;

public class DashCycleTests
{
    [Test]
    public void Cycles_Aiming_Dashing_Recovering_Aiming_on_the_configured_timers()
    {
        var c = new DashCycle(1f, 1f, 1f);
        Assert.AreEqual(DashCycle.Phase.Aiming, c.Tick(0.5f).Phase);
        Assert.AreEqual(DashCycle.Phase.Dashing, c.Tick(0.5f).Phase);
        c.Tick(0.5f);
        Assert.AreEqual(DashCycle.Phase.Recovering, c.Tick(0.5f).Phase);
        c.Tick(0.5f);
        Assert.AreEqual(DashCycle.Phase.Aiming, c.Tick(0.5f).Phase);
    }

    [Test]
    public void StartedDash_is_true_for_exactly_one_tick_entering_Dashing()
    {
        var c = new DashCycle(1f, 1f, 1f);
        Assert.IsFalse(c.Tick(0.5f).StartedDash);
        Assert.IsTrue(c.Tick(0.5f).StartedDash);
        Assert.IsFalse(c.Tick(0.5f).StartedDash);
    }

    [Test]
    public void Telegraphing_is_true_only_during_Aiming()
    {
        var c = new DashCycle(1f, 1f, 1f);
        Assert.IsTrue(c.Tick(0.5f).Telegraphing);  // Aiming
        Assert.IsFalse(c.Tick(0.5f).Telegraphing); // Dashing
        Assert.IsFalse(c.Tick(0.5f).Telegraphing); // Dashing
    }
}
