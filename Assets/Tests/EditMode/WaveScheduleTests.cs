using NUnit.Framework;
using VacuumSurvivor;

public class WaveScheduleTests
{
    static WaveEntry[] Entries() => new[]
    {
        new WaveEntry { startTime = 0f },
        new WaveEntry { startTime = 30f },
        new WaveEntry { startTime = 75f },
    };

    [Test]
    public void Before_first_returns_first()
    {
        var e = Entries();
        Assert.AreSame(e[0], WaveSchedule.Current(-5f, e));
    }

    [Test]
    public void Picks_latest_started_wave()
    {
        var e = Entries();
        Assert.AreSame(e[1], WaveSchedule.Current(40f, e));
    }

    [Test]
    public void Start_boundary_is_inclusive()
    {
        var e = Entries();
        Assert.AreSame(e[1], WaveSchedule.Current(30f, e));
    }

    [Test]
    public void After_all_returns_last()
    {
        var e = Entries();
        Assert.AreSame(e[2], WaveSchedule.Current(999f, e));
    }
}
