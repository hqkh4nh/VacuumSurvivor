using NUnit.Framework;
using VacuumSurvivor;

public class PowerUpRollTests
{
    [Test]
    public void ShouldDrop_is_true_below_the_chance_and_false_at_or_above_it()
    {
        Assert.IsTrue(PowerUpRoll.ShouldDrop(0.049, 0.05f));
        Assert.IsFalse(PowerUpRoll.ShouldDrop(0.051, 0.05f));
        Assert.IsFalse(PowerUpRoll.ShouldDrop(0.9, 0.05f));
    }

    [Test]
    public void Pick_maps_the_three_thirds_to_Battery_Magnet_Bomb()
    {
        Assert.AreEqual(PowerUpKind.Battery, PowerUpRoll.Pick(0.1));
        Assert.AreEqual(PowerUpKind.Magnet, PowerUpRoll.Pick(0.5));
        Assert.AreEqual(PowerUpKind.Bomb, PowerUpRoll.Pick(0.9));
    }
}
