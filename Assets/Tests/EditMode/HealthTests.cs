using NUnit.Framework;
using VacuumSurvivor;

public class HealthTests
{
    [Test]
    public void Damage_reduces_current()
    {
        var h = new Health(30f);
        h.Damage(10f);
        Assert.AreEqual(20f, h.Current, 1e-4f);
    }

    [Test]
    public void Is_dead_at_zero()
    {
        var h = new Health(10f);
        h.Damage(10f);
        Assert.IsTrue(h.IsDead);
    }

    [Test]
    public void Current_never_goes_negative()
    {
        var h = new Health(10f);
        h.Damage(50f);
        Assert.AreEqual(0f, h.Current);
    }
}
