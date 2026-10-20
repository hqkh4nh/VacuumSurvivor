using NUnit.Framework;
using UnityEngine;
using VacuumSurvivor;

public class SpriteFactoryTests
{
    static float[] FullSquare(int px)
    {
        var m = new float[px * px];
        for (int i = 0; i < m.Length; i++) m[i] = 1f;
        return m;
    }

    static float Lum(Color c) => c.r + c.g + c.b;

    [Test]
    public void Shade_outlines_the_mask_edge_with_ink()
    {
        int px = 16;
        var outp = SpriteFactory.Shade(FullSquare(px), px, new Color(0.5f, 0.5f, 0.5f));
        var corner = outp[0]; // (0,0) sits on the texture edge
        Assert.AreEqual(Palette.Ink.r, corner.r, 0.01f);
        Assert.AreEqual(Palette.Ink.g, corner.g, 0.01f);
        Assert.AreEqual(Palette.Ink.b, corner.b, 0.01f);
    }

    [Test]
    public void Shade_lightens_the_top_band_and_darkens_the_bottom_band()
    {
        int px = 32;
        var outp = SpriteFactory.Shade(FullSquare(px), px, new Color(0.5f, 0.5f, 0.5f));
        var top = outp[28 * px + 16];
        var bottom = outp[2 * px + 16];
        Assert.Greater(Lum(top), Lum(bottom));
    }

    [Test]
    public void Shade_leaves_uncovered_pixels_transparent()
    {
        int px = 16;
        var mask = new float[px * px];
        float r = px * 0.4f, c = px * 0.5f;
        for (int y = 0; y < px; y++)
        for (int x = 0; x < px; x++)
        {
            float dx = x - c + 0.5f, dy = y - c + 0.5f;
            mask[y * px + x] = dx * dx + dy * dy <= r * r ? 1f : 0f;
        }
        var outp = SpriteFactory.Shade(mask, px, new Color(0.5f, 0.5f, 0.5f));
        Assert.AreEqual(0f, outp[0].a, 0.001f); // corner (0,0) is outside the disc
    }
}
