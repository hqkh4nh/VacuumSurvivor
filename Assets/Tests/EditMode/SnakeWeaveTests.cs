using NUnit.Framework;
using UnityEngine;
using VacuumSurvivor;

public class SnakeWeaveTests
{
    [Test]
    public void Direction_stays_within_90_degrees_of_the_toward_vector_across_a_full_period()
    {
        var toward = new Vector2(1f, 0f);
        for (int i = 0; i <= 16; i++)
        {
            float t = (2f * Mathf.PI) * (i / 16f);
            var dir = SnakeWeave.Direction(toward, t, 0.6f, 1f);
            Assert.Greater(Vector2.Dot(dir, toward.normalized), 0f);
        }
    }

    [Test]
    public void Perpendicular_offset_flips_sign_between_a_quarter_and_three_quarter_period()
    {
        var toward = new Vector2(1f, 0f);
        var perp = new Vector2(0f, 1f);
        float quarter = Mathf.PI * 0.5f;        // sin = +1
        float threeQuarter = Mathf.PI * 1.5f;   // sin = -1
        float a = Vector2.Dot(SnakeWeave.Direction(toward, quarter, 0.6f, 1f), perp);
        float b = Vector2.Dot(SnakeWeave.Direction(toward, threeQuarter, 0.6f, 1f), perp);
        Assert.Greater(a, 0f);
        Assert.Less(b, 0f);
    }
}
