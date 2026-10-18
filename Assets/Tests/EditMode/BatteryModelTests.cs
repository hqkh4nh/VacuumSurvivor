using NUnit.Framework;
using VacuumSurvivor;

public class BatteryModelTests
{
    [Test]
    public void Tick_drains_by_rate_times_dt()
    {
        var b = new BatteryModel(100f);
        b.Tick(2f, 0.25f);
        Assert.AreEqual(99.5f, b.Current, 1e-4f);
    }

    [Test]
    public void Damage_reduces_current()
    {
        var b = new BatteryModel(100f);
        b.Damage(30f);
        Assert.AreEqual(70f, b.Current, 1e-4f);
    }

    [Test]
    public void Current_clamps_at_zero_and_reports_dead()
    {
        var b = new BatteryModel(100f);
        b.Damage(200f);
        Assert.AreEqual(0f, b.Current);
        Assert.IsTrue(b.IsDead);
    }

    [Test]
    public void Charge_clamps_at_max()
    {
        var b = new BatteryModel(100f);
        b.Charge(10f, 100f);
        Assert.AreEqual(100f, b.Current);
    }

    [Test]
    public void Full_battery_is_not_dead()
    {
        var b = new BatteryModel(100f);
        Assert.IsFalse(b.IsDead);
    }
}
