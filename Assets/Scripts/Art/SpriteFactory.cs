using UnityEngine;

namespace VacuumSurvivor
{
    /// Builds the game's placeholder look at runtime: a filled circle with two glowing eyes,
    /// so the project needs no imported art and keeps the dark "robot in the dark" mood.
    public static class SpriteFactory
    {
        const int PixelsPerUnit = 64;

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
