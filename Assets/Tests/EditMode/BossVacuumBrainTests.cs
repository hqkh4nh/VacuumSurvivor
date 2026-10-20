using System.Collections.Generic;
using NUnit.Framework;
using VacuumSurvivor;

public class BossVacuumBrainTests
{
    // Records the distinct run of phases a brain passes through over `seconds`, ticking at `dt`.
    static List<BossVacuumBrain.Phase> PhaseRun(BossVacuumBrain brain, float seconds, float dt)
    {
        var run = new List<BossVacuumBrain.Phase>();
        for (float t = 0f; t < seconds; t += dt)
        {
            var p = brain.Tick(dt).Phase;
            if (run.Count == 0 || run[run.Count - 1] != p) run.Add(p);
        }
        return run;
    }

    [Test]
    public void Starts_in_suck_with_pulling()
    {
        var d = new BossVacuumBrain().Tick(0.1f);
        Assert.AreEqual(BossVacuumBrain.Phase.Suck, d.Phase);
        Assert.IsTrue(d.Pulling);
    }

    [Test]
    public void Blow_burst_fires_exactly_once_per_cycle()
    {
        var brain = new BossVacuumBrain();
        int bursts = 0;
        // 8.5s covers one full cycle (~9.1s wall but the blow lands at ~4.5s) without reaching
        // the next cycle's blow.
        for (float t = 0f; t < 8.5f; t += 0.05f)
            if (brain.Tick(0.05f).BlowBurst) bursts++;
        Assert.AreEqual(1, bursts);
    }

    [Test]
    public void Telegraph_precedes_dash()
    {
        var brain = new BossVacuumBrain();
        bool sawTelegraph = false;
        for (float t = 0f; t < 9f; t += 0.05f)
        {
            var d = brain.Tick(0.05f);
            if (d.DashTelegraphing) sawTelegraph = true;
            if (d.DashActive) { Assert.IsTrue(sawTelegraph, "dash began before any telegraph"); return; }
        }
        Assert.Fail("dash phase never reached");
    }

    [Test]
    public void Rests_separate_every_attack()
    {
        var run = PhaseRun(new BossVacuumBrain(), 9.2f, 0.05f);
        var expected = new[]
        {
            BossVacuumBrain.Phase.Suck, BossVacuumBrain.Phase.RestAfterSuck,
            BossVacuumBrain.Phase.Blow, BossVacuumBrain.Phase.RestAfterBlow,
            BossVacuumBrain.Phase.DashTelegraph, BossVacuumBrain.Phase.Dash,
            BossVacuumBrain.Phase.RestAfterDash,
        };
        for (int i = 0; i < expected.Length; i++) Assert.AreEqual(expected[i], run[i]);
    }

    [Test]
    public void Cycle_repeats_in_order()
    {
        var run = PhaseRun(new BossVacuumBrain(), 17f, 0.05f);
        // The cycle is ~9.15s, so 17s spans exactly two Suck entries.
        int sucks = run.FindAll(p => p == BossVacuumBrain.Phase.Suck).Count;
        Assert.AreEqual(2, sucks);
    }
}
