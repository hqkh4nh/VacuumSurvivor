using NUnit.Framework;
using VacuumSurvivor;

public class BatteryColorTests
{
    [Test]
    public void Above_60_percent_is_green() =>
        Assert.AreEqual(BatteryColor.Green, BatteryColor.For(0.8f));

    [Test]
    public void At_60_percent_is_yellow() =>
        Assert.AreEqual(BatteryColor.Yellow, BatteryColor.For(0.6f));

    [Test]
    public void Between_30_and_60_is_yellow() =>
        Assert.AreEqual(BatteryColor.Yellow, BatteryColor.For(0.45f));

    [Test]
    public void Below_30_percent_is_red() =>
        Assert.AreEqual(BatteryColor.Red, BatteryColor.For(0.2f));
}
