using NUnit.Framework;
using VacuumSurvivor;

public class BossCatBrainTests
{
    [Test]
    public void Sheds_on_the_shed_interval()
    {
        var b = new BossCatBrain(); // shedInterval 3
        Assert.IsFalse(b.Tick(1f, 100f, 1f).Shed);
        Assert.IsFalse(b.Tick(1f, 100f, 1f).Shed);
        Assert.IsTrue(b.Tick(1f, 100f, 1f).Shed);
    }

    [Test]
    public void Enraged_below_thirty_percent_shortens_shed_interval_and_speeds_up()
    {
        var b = new BossCatBrain();
        var first = b.Tick(1f, 100f, 0.2f);
        Assert.IsTrue(first.Enraged);
        Assert.AreEqual(1.5f, first.SpeedMult, 0.0001f);
        Assert.IsFalse(first.Shed);
        Assert.IsTrue(b.Tick(1f, 100f, 0.2f).Shed); // 2s interval when enraged
    }

    [Test]
    public void Does_not_enter_windup_when_player_out_of_range()
    {
        var b = new BossCatBrain(); // slapCooldown 6, range 3
        var d = b.Tick(6f, 10f, 1f);
        Assert.IsFalse(d.Telegraphing);
        Assert.IsFalse(d.SlapHit);
        Assert.AreEqual(BossCatBrain.Phase.Roaming, b.Current);
    }

    [Test]
    public void Enters_windup_then_hits_after_the_windup_delay()
    {
        var b = new BossCatBrain(100f, 100f, 1f, 3f, 1f); // cooldown 1, windup 1
        b.Tick(0.6f, 2f, 1f);
        var entering = b.Tick(0.6f, 2f, 1f);
        Assert.IsTrue(entering.Telegraphing);
        Assert.IsFalse(entering.SlapHit);
        Assert.AreEqual(BossCatBrain.Phase.SlapWindup, b.Current);
        var hit = b.Tick(0.6f, 2f, 1f);
        Assert.IsTrue(hit.SlapHit);
        Assert.AreEqual(BossCatBrain.Phase.Roaming, b.Current);
    }

    [Test]
    public void Slap_misses_when_player_leaves_range_during_windup()
    {
        var b = new BossCatBrain(100f, 100f, 1f, 3f, 1f);
        b.Tick(0.6f, 2f, 1f);
        b.Tick(0.6f, 2f, 1f); // enters windup
        var d = b.Tick(0.6f, 10f, 1f); // player fled before the hit frame
        Assert.IsFalse(d.SlapHit);
        Assert.AreEqual(BossCatBrain.Phase.Roaming, b.Current);
    }

    [Test]
    public void Does_not_slap_again_until_cooldown_elapses()
    {
        var b = new BossCatBrain(100f, 100f, 1f, 3f, 1f);
        b.Tick(0.6f, 2f, 1f);
        b.Tick(0.6f, 2f, 1f); // windup
        b.Tick(0.6f, 2f, 1f); // hit, cooldown resets
        var d = b.Tick(0.6f, 2f, 1f); // only 0.6s since the slap, cooldown is 1s
        Assert.IsFalse(d.Telegraphing);
        Assert.AreEqual(BossCatBrain.Phase.Roaming, b.Current);
    }
}
