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
