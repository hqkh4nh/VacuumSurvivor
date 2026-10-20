using UnityEngine;

namespace VacuumSurvivor
{
    public enum SpriteKind { Robot, DustBunny, Breadcrumb, HairClump, Cat, DustMote, Dock, Led, Ring }

    /// Builds the game's placeholder look at runtime: a filled circle with two glowing eyes,
    /// so the project needs no imported art and keeps the dark "robot in the dark" mood.
    public static class SpriteFactory
    {
        const int PixelsPerUnit = 64;

        /// Turns a coverage mask (row-major, 0..1) into shaded pixels under one fixed light: the top
        /// 40% is tinted toward moonlight, the bottom 12% is darkened, and the silhouette edge is
        /// inked. Hard bands, not a gradient - that flat 2-tone is what reads as drawn rather than
        /// rendered. Every generated sprite goes through this so the whole set matches.
        public static Color[] Shade(float[] mask, int px, Color body)
        {
            var outp = new Color[px * px];
            var clear = new Color(0f, 0f, 0f, 0f);
            float topStart = 0.60f * px;
            float botEnd = 0.12f * px;

            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                int i = y * px + x;
                float cov = mask[i];
                if (cov <= 0f) { outp[i] = clear; continue; }

                Color c = body;
                if (y >= topStart) c = Color.Lerp(body, Moonlight, 0.25f);
                else if (y < botEnd) c = new Color(body.r * 0.65f, body.g * 0.65f, body.b * 0.65f);
                if (IsEdge(mask, px, x, y)) c = Ink;

                c.a = body.a * cov;
                outp[i] = c;
            }
            return outp;
        }

        static readonly Color Moonlight = Palette.Moonlight;
        static readonly Color Ink = Palette.Ink;

        static bool IsEdge(float[] mask, int px, int x, int y)
        {
            if (x == 0 || y == 0 || x == px - 1 || y == px - 1) return true;
            return mask[y * px + x - 1] <= 0f || mask[y * px + x + 1] <= 0f
                || mask[(y - 1) * px + x] <= 0f || mask[(y + 1) * px + x] <= 0f;
        }

        /// The one entry point: each kind builds its own silhouette, so shape (not colour or glow)
        /// is what tells the creatures apart.
        public static Sprite Build(SpriteKind kind, Color body, int px)
        {
            switch (kind)
            {
                case SpriteKind.Robot: return Robot(body, px);
                case SpriteKind.DustBunny: return DustBunny(body, px);
                case SpriteKind.Breadcrumb: return Breadcrumb(body, px);
                case SpriteKind.HairClump: return HairClump(body, px);
                case SpriteKind.Cat: return Cat(body, px);
                case SpriteKind.DustMote: return DustMote(body, px);
                case SpriteKind.Dock: return Dock(body, px);
                default: return PlainCircle(body, px); // Led, Ring
            }
        }

        static readonly Color EyeGlow = new Color(0.86f, 0.95f, 1f, 1f);

        static Sprite Robot(Color body, int px)
        {
            var m = NewMask(px);
            RoundedRect(m, px, px * 0.5f, px * 0.47f, px * 0.34f, px * 0.33f, px * 0.12f);
            return Finish(Shade(m, px, body), px, tex =>
            {
                FillRectTex(tex, px, 0.30f, 0.70f, 0.13f, 0.21f, Palette.Ink); // front suction slot
                DrawDot(tex, px * 0.5f, px * 0.62f, px * 0.09f, new Color(0.70f, 0.92f, 1f, 1f)); // lens
            });
        }

        static Sprite DustBunny(Color body, int px)
        {
            var m = NewMask(px);
            Fuzzy(m, px, px * 0.5f, px * 0.48f, px * 0.36f);
            return Finish(Shade(m, px, body), px, tex => SmallEyes(tex, px));
        }

        static Sprite Breadcrumb(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            TriMask(m, px, new Vector2(c * 0.16f, c * 0.42f), new Vector2(c * 0.52f, c * 0.30f), new Vector2(c * 0.40f, c * 0.72f));
            TriMask(m, px, new Vector2(c * 0.44f, c * 0.34f), new Vector2(c * 0.84f, c * 0.46f), new Vector2(c * 0.60f, c * 0.76f));
            TriMask(m, px, new Vector2(c * 0.30f, c * 0.54f), new Vector2(c * 0.64f, c * 0.58f), new Vector2(c * 0.46f, c * 0.88f));
            return Finish(Shade(m, px, body), px, tex => SmallEyes(tex, px));
        }

        static Sprite HairClump(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Ellipse(m, px, c * 0.5f, c * 0.47f, c * 0.33f, c * 0.27f);
            TriMask(m, px, new Vector2(c * 0.5f, c * 0.47f), new Vector2(c * 0.08f, c * 0.62f), new Vector2(c * 0.20f, c * 0.38f));
            TriMask(m, px, new Vector2(c * 0.5f, c * 0.47f), new Vector2(c * 0.92f, c * 0.60f), new Vector2(c * 0.80f, c * 0.40f));
            TriMask(m, px, new Vector2(c * 0.5f, c * 0.52f), new Vector2(c * 0.40f, c * 0.94f), new Vector2(c * 0.58f, c * 0.90f));
            TriMask(m, px, new Vector2(c * 0.44f, c * 0.44f), new Vector2(c * 0.18f, c * 0.14f), new Vector2(c * 0.34f, c * 0.22f));
            return Finish(Shade(m, px, body), px, tex => SmallEyes(tex, px));
        }

        static Sprite Cat(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Ellipse(m, px, c * 0.48f, c * 0.44f, c * 0.34f, c * 0.30f);
            TriMask(m, px, new Vector2(c * 0.22f, c * 0.62f), new Vector2(c * 0.42f, c * 0.66f), new Vector2(c * 0.28f, c * 0.92f));
            TriMask(m, px, new Vector2(c * 0.74f, c * 0.62f), new Vector2(c * 0.54f, c * 0.66f), new Vector2(c * 0.68f, c * 0.92f));
            for (int i = 0; i <= 6; i++) // tail curling up from the lower right
            {
                float t = i / 6f, ang = Mathf.Lerp(-0.4f, 1.4f, t);
                Disc(m, px, c * 0.80f + Mathf.Cos(ang) * c * 0.17f, c * 0.30f + Mathf.Sin(ang) * c * 0.22f, c * (0.075f - 0.035f * t));
            }
            return Finish(Shade(m, px, body), px, tex => BigEyes(tex, px));
        }

        static Sprite DustMote(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Disc(m, px, c * 0.5f, c * 0.5f, c * 0.16f);
            TriMask(m, px, new Vector2(c * 0.44f, c * 0.5f), new Vector2(c * 0.56f, c * 0.5f), new Vector2(c * 0.5f, c * 0.96f));
            TriMask(m, px, new Vector2(c * 0.44f, c * 0.5f), new Vector2(c * 0.56f, c * 0.5f), new Vector2(c * 0.5f, c * 0.04f));
            TriMask(m, px, new Vector2(c * 0.5f, c * 0.44f), new Vector2(c * 0.5f, c * 0.56f), new Vector2(c * 0.96f, c * 0.5f));
            TriMask(m, px, new Vector2(c * 0.5f, c * 0.44f), new Vector2(c * 0.5f, c * 0.56f), new Vector2(c * 0.04f, c * 0.5f));
            return Finish(Shade(m, px, body), px, tex => DrawDot(tex, c * 0.5f, c * 0.5f, c * 0.11f, new Color(1f, 0.85f, 0.94f, 1f)));
        }

        static Sprite Dock(Color body, int px)
        {
            var m = NewMask(px);
            RoundedRect(m, px, px * 0.5f, px * 0.5f, px * 0.42f, px * 0.22f, px * 0.10f);
            return Finish(Shade(m, px, body), px, tex => DrawRing(tex, px, px * 0.5f, px * 0.5f, px * 0.34f, px * 0.16f, Palette.Accent));
        }

        static Sprite PlainCircle(Color body, int px)
        {
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float r = px * 0.5f;
            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                tex.SetPixel(x, y, a <= 0f ? clear : new Color(body.r, body.g, body.b, body.a * a));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        static Sprite Finish(Color[] pixels, int px, System.Action<Texture2D> overlay)
        {
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            tex.SetPixels(pixels);
            overlay?.Invoke(tex);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        static float[] NewMask(int px) => new float[px * px];

        static void Disc(float[] m, int px, float cx, float cy, float r)
        {
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = x - cx + 0.5f, dy = y - cy + 0.5f;
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                int i = y * px + x; if (a > m[i]) m[i] = a;
            }
        }

        static void Ellipse(float[] m, int px, float cx, float cy, float rx, float ry)
        {
            float soft = Mathf.Min(rx, ry);
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = (x - cx + 0.5f) / rx, dy = (y - cy + 0.5f) / ry;
                float a = Mathf.Clamp01((1f - Mathf.Sqrt(dx * dx + dy * dy)) * soft);
                int i = y * px + x; if (a > m[i]) m[i] = a;
            }
        }

        static void RoundedRect(float[] m, int px, float cx, float cy, float hw, float hh, float rad)
        {
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float qx = Mathf.Abs(x - cx + 0.5f) - (hw - rad);
                float qy = Mathf.Abs(y - cy + 0.5f) - (hh - rad);
                float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
                float dist = Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - rad;
                float a = Mathf.Clamp01(-dist);
                int i = y * px + x; if (a > m[i]) m[i] = a;
            }
        }

        static void TriMask(float[] m, int px, Vector2 a, Vector2 b, Vector2 c)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))), 0, px - 1);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))), 0, px - 1);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))), 0, px - 1);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))), 0, px - 1);
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
                bool neg = d1 < 0f || d2 < 0f || d3 < 0f, pos = d1 > 0f || d2 > 0f || d3 > 0f;
                if (!(neg && pos)) m[y * px + x] = 1f;
            }
        }

        static void Fuzzy(float[] m, int px, float cx, float cy, float baseR)
        {
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = x - cx + 0.5f, dy = y - cy + 0.5f;
                float ang = Mathf.Atan2(dy, dx);
                float r = baseR * (1f + 0.16f * Mathf.Sin(5f * ang) + 0.11f * Mathf.Sin(9f * ang + 1.3f) + 0.07f * Mathf.Sin(13f * ang + 0.5f));
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                int i = y * px + x; if (a > m[i]) m[i] = a;
            }
        }

        static void FillRectTex(Texture2D tex, int px, float xMin, float xMax, float yMin, float yMax, Color col)
        {
            int x0 = Mathf.RoundToInt(xMin * px), x1 = Mathf.RoundToInt(xMax * px);
            int y0 = Mathf.RoundToInt(yMin * px), y1 = Mathf.RoundToInt(yMax * px);
            for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
                if (x >= 0 && y >= 0 && x < px && y < px && tex.GetPixel(x, y).a > 0f) tex.SetPixel(x, y, col);
        }

        static void DrawRing(Texture2D tex, int px, float cx, float cy, float rx, float ry, Color col)
        {
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = (x - cx + 0.5f) / rx, dy = (y - cy + 0.5f) / ry;
                if (Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - 1f) < 0.09f && tex.GetPixel(x, y).a > 0f)
                    tex.SetPixel(x, y, col);
            }
        }

        static void SmallEyes(Texture2D tex, int px)
        {
            DrawDot(tex, px * 0.40f, px * 0.56f, px * 0.055f, EyeGlow);
            DrawDot(tex, px * 0.60f, px * 0.56f, px * 0.055f, EyeGlow);
        }

        static void BigEyes(Texture2D tex, int px)
        {
            DrawDot(tex, px * 0.40f, px * 0.54f, px * 0.08f, EyeGlow);
            DrawDot(tex, px * 0.60f, px * 0.54f, px * 0.08f, EyeGlow);
        }

        public static Sprite CircleWithEyes(Color body, int px = 64, bool eyes = true)
        {
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float r = px * 0.5f;
            var clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                // One-pixel soft edge so circles don't look jagged at small sizes.
                float a = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, a <= 0f ? clear : new Color(body.r, body.g, body.b, body.a * a));
            }

            if (eyes)
            {
                var glow = new Color(0.86f, 0.95f, 1f, 1f);
                DrawDot(tex, px * 0.37f, px * 0.6f, px * 0.1f, glow);
                DrawDot(tex, px * 0.63f, px * 0.6f, px * 0.1f, glow);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        /// A placeholder boss cat seen from above: a big round body with two triangular ears and
        /// the same glowing eyes as the other creatures.
        public static Sprite CatSprite(Color body, int px = 96)
        {
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var clear = new Color(0f, 0f, 0f, 0f);
            float cx = px * 0.5f, cy = px * 0.46f, r = px * 0.42f;

            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = x - cx + 0.5f, dy = y - cy + 0.5f;
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                tex.SetPixel(x, y, a <= 0f ? clear : new Color(body.r, body.g, body.b, body.a * a));
            }

            FillTriangle(tex, new Vector2(px * 0.18f, px * 0.64f), new Vector2(px * 0.42f, px * 0.70f), new Vector2(px * 0.26f, px * 0.96f), body);
            FillTriangle(tex, new Vector2(px * 0.82f, px * 0.64f), new Vector2(px * 0.58f, px * 0.70f), new Vector2(px * 0.74f, px * 0.96f), body);

            var glow = new Color(0.86f, 0.95f, 1f, 1f);
            DrawDot(tex, px * 0.38f, px * 0.54f, px * 0.08f, glow);
            DrawDot(tex, px * 0.62f, px * 0.54f, px * 0.08f, glow);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 c, Color col)
        {
            int x0 = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
            int x1 = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
            int y0 = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
            int y1 = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)));
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
                bool neg = d1 < 0f || d2 < 0f || d3 < 0f;
                bool pos = d1 > 0f || d2 > 0f || d3 > 0f;
                if (!(neg && pos)) tex.SetPixel(x, y, col);
            }
        }

        static float Sign(Vector2 p, Vector2 a, Vector2 b) =>
            (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);

        static void DrawDot(Texture2D tex, float cx, float cy, float rad, Color col)
        {
            int x0 = Mathf.FloorToInt(cx - rad), x1 = Mathf.CeilToInt(cx + rad);
            int y0 = Mathf.FloorToInt(cy - rad), y1 = Mathf.CeilToInt(cy + rad);
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                float dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= rad * rad) tex.SetPixel(x, y, col);
            }
        }
    }
}
