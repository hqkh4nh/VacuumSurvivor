using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VacuumSurvivor;

public class TargetingTests
{
    [Test]
    public void Picks_the_closest_point()
    {
        var points = new List<Vector2> { new Vector2(5f, 0f), new Vector2(1f, 0f), new Vector2(3f, 0f) };
        Assert.AreEqual(1, Targeting.Nearest(Vector2.zero, points));
    }

    [Test]
    public void Empty_returns_minus_one()
    {
        Assert.AreEqual(-1, Targeting.Nearest(Vector2.zero, new List<Vector2>()));
    }

    [Test]
    public void Single_point_returns_zero()
    {
        var points = new List<Vector2> { new Vector2(9f, 9f) };
        Assert.AreEqual(0, Targeting.Nearest(Vector2.zero, points));
    }

    [Test]
    public void Measures_from_the_given_origin()
    {
        var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(10f, 10f) };
        Assert.AreEqual(1, Targeting.Nearest(new Vector2(11f, 11f), points));
    }
}
