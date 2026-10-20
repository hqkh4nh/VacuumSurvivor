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

    static readonly SpriteKind[] Creatures =
        { SpriteKind.Robot, SpriteKind.DustBunny, SpriteKind.Breadcrumb, SpriteKind.HairClump, SpriteKind.Cat,
          SpriteKind.Sock, SpriteKind.Lego, SpriteKind.Cord };

    static Texture2D Tex(SpriteKind k, Color body, int px) => (Texture2D)SpriteFactory.Build(k, body, px).texture;

    static int OpaqueCount(Texture2D t)
    {
        int n = 0;
        foreach (var c in t.GetPixels()) if (c.a > 0.01f) n++;
        return n;
    }

    static bool[] AlphaMask(Texture2D t)
    {
        var px = t.GetPixels();
        var a = new bool[px.Length];
        for (int i = 0; i < px.Length; i++) a[i] = px[i].a > 0.5f;
        return a;
    }

    static bool SameMask(bool[] a, bool[] b)
    {
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    [Test]
    public void Each_creature_kind_returns_a_non_null_sprite_of_the_requested_size()
    {
        const int px = 48;
        foreach (var k in Creatures)
        {
            var s = SpriteFactory.Build(k, Color.gray, px);
            Assert.IsNotNull(s, k.ToString());
            Assert.AreEqual(px, (int)s.rect.width, k.ToString());
            Assert.AreEqual(px, (int)s.rect.height, k.ToString());
        }
    }

    [Test]
    public void Each_creature_kind_covers_pixels_and_stays_within_bounds()
    {
        const int px = 48;
        foreach (var k in Creatures)
        {
            var t = Tex(k, Color.gray, px);
            Assert.AreEqual(px, t.width, k.ToString());
            Assert.AreEqual(px, t.height, k.ToString());
            Assert.Greater(OpaqueCount(t), 0, k.ToString());
        }
    }

    [Test]
    public void The_creature_kinds_are_not_all_the_same_silhouette()
    {
        const int px = 48;
        var robot = AlphaMask(Tex(SpriteKind.Robot, Color.gray, px));
        var bunny = AlphaMask(Tex(SpriteKind.DustBunny, Color.gray, px));
        var crumb = AlphaMask(Tex(SpriteKind.Breadcrumb, Color.gray, px));
        Assert.IsFalse(SameMask(robot, bunny) && SameMask(bunny, crumb));
    }

    [Test]
    public void Dock_and_dust_mote_return_non_null_in_bounds_sprites()
    {
        var dock = SpriteFactory.Build(SpriteKind.Dock, Color.white, 64);
        Assert.IsNotNull(dock);
        Assert.AreEqual(64, (int)dock.rect.width);
        Assert.Greater(OpaqueCount((Texture2D)dock.texture), 0);

        var mote = SpriteFactory.Build(SpriteKind.DustMote, Color.white, 16);
        Assert.IsNotNull(mote);
        Assert.AreEqual(16, (int)mote.rect.width);
        Assert.Greater(OpaqueCount((Texture2D)mote.texture), 0);
    }

    [Test]
    public void Broom_and_droplet_return_non_null_in_bounds_sprites_with_coverage()
    {
        foreach (var k in new[] { SpriteKind.Broom, SpriteKind.Droplet })
        {
            const int px = 24;
            var s = SpriteFactory.Build(k, Color.gray, px);
            Assert.IsNotNull(s, k.ToString());
            Assert.AreEqual(px, (int)s.rect.width, k.ToString());
            Assert.Greater(OpaqueCount((Texture2D)s.texture), 0, k.ToString());
        }
    }

    [Test]
    public void Broom_and_droplet_have_different_silhouettes()
    {
        const int px = 24;
        var broom = AlphaMask(Tex(SpriteKind.Broom, Color.gray, px));
        var drop = AlphaMask(Tex(SpriteKind.Droplet, Color.gray, px));
        Assert.IsFalse(SameMask(broom, drop));
    }

    [Test]
    public void Vacuum_boss_returns_a_non_null_in_bounds_sprite_with_coverage()
    {
        const int px = 64;
        var s = SpriteFactory.Build(SpriteKind.Vacuum, Color.gray, px);
        Assert.IsNotNull(s);
        Assert.AreEqual(px, (int)s.rect.width);
        Assert.Greater(OpaqueCount((Texture2D)s.texture), 0);
    }

    [Test]
    public void Puddle_and_mop_trail_return_non_null_in_bounds_sprites_with_coverage()
    {
        foreach (var k in new[] { SpriteKind.Puddle, SpriteKind.MopTrail })
        {
            const int px = 48;
            var s = SpriteFactory.Build(k, Color.gray, px);
            Assert.IsNotNull(s, k.ToString());
            Assert.AreEqual(px, (int)s.rect.width, k.ToString());
            Assert.Greater(OpaqueCount((Texture2D)s.texture), 0, k.ToString());
        }
    }

    [Test]
    public void Powerup_items_return_non_null_in_bounds_sprites_with_coverage()
    {
        foreach (var k in new[] { SpriteKind.Battery, SpriteKind.Magnet, SpriteKind.Bomb })
        {
            const int px = 32;
            var s = SpriteFactory.Build(k, Color.gray, px);
            Assert.IsNotNull(s, k.ToString());
            Assert.AreEqual(px, (int)s.rect.width, k.ToString());
            Assert.Greater(OpaqueCount((Texture2D)s.texture), 0, k.ToString());
        }
    }

    [Test]
    public void Led_and_ring_are_plain_filled_circles()
    {
        const int px = 32;
        foreach (var k in new[] { SpriteKind.Led, SpriteKind.Ring })
        {
            var t = Tex(k, Color.white, px);
            Assert.Greater(t.GetPixel(px / 2, px / 2).r, 0.8f, k.ToString()); // bright body, not darkened
            bool hasInk = false;
            foreach (var c in t.GetPixels())
                if (c.a > 0.5f && Mathf.Abs(c.r - Palette.Ink.r) < 0.03f && Mathf.Abs(c.g - Palette.Ink.g) < 0.03f) hasInk = true;
            Assert.IsFalse(hasInk, k + " should not be inked (plain, unshaded)");
        }
    }

    [Test]
    public void Floor_tile_is_non_null_requested_size_and_fully_opaque()
    {
        const int px = 64;
        var s = SpriteFactory.FloorTile(px);
        Assert.IsNotNull(s);
        Assert.AreEqual(px, (int)s.rect.width);
        foreach (var c in ((Texture2D)s.texture).GetPixels()) Assert.AreEqual(1f, c.a, 0.001f);
    }
}
