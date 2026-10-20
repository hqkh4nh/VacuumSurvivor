using UnityEngine;

namespace VacuumSurvivor
{
    public enum SpriteKind { Robot, DustBunny, Breadcrumb, HairClump, Cat, DustMote, Dock, Led, Ring, Broom, Droplet, Vacuum, Sock, Lego, Puddle, Cord, MopTrail, Battery, Magnet, Bomb }

    /// Builds the whole game's look at runtime: one shared flat-shading model over a distinct
    /// silhouette per entity, so the project needs no imported art and keeps the "moonlit" mood.
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
                case SpriteKind.Broom: return Broom(body, px);
                case SpriteKind.Droplet: return Droplet(body, px);
                case SpriteKind.Vacuum: return Vacuum(body, px);
                case SpriteKind.Sock: return Sock(body, px);
                case SpriteKind.Lego: return Lego(body, px);
                case SpriteKind.Puddle: return Puddle(body, px);
                case SpriteKind.Cord: return Cord(body, px);
                case SpriteKind.MopTrail: return MopTrail(body, px);
                case SpriteKind.Battery: return Battery(body, px);
                case SpriteKind.Magnet: return Magnet(body, px);
                case SpriteKind.Bomb: return Bomb(body, px);
                default: return PlainCircle(body, px); // Led, Ring
            }
        }

        /// A low-contrast parquet tile for the floor: warm boards in a half-offset brick layout with
        /// darker seams, built to repeat seamlessly under a tiled SpriteRenderer so the scene reads as
        /// a room at night instead of an empty void. Fully opaque.
        public static Sprite FloorTile(int px = 256)
        {
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            int plankH = px / 2;
            int seam = Mathf.Max(1, px / 128);
            var board = Palette.FloorBoard;
            var row1 = new Color(board.r * 0.94f, board.g * 0.94f, board.b * 0.94f);
            var seamC = Palette.FloorSeam;

            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                bool bottomRow = y < plankH;
                int vSeamX = bottomRow ? 0 : px / 2; // the upper row is offset half a plank
                bool vSeam = (x - vSeamX + px) % px < seam;
                bool hSeam = y % plankH < seam;
                Color c = hSeam || vSeam ? seamC : (bottomRow ? board : row1);
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, px, px), new Vector2(0.5f, 0.5f), px / 4f);
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
            Fuzzy(m, px, px * 0.5f, px * 0.48f, px * 0.39f);
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

        static Sprite Broom(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            RoundedRect(m, px, c * 0.5f, c * 0.40f, c * 0.055f, c * 0.34f, c * 0.05f); // handle
            TriMask(m, px, new Vector2(c * 0.27f, c * 0.98f), new Vector2(c * 0.73f, c * 0.98f), new Vector2(c * 0.5f, c * 0.58f)); // bristle fan
            return Finish(Shade(m, px, body), px, null); // a tool, not a creature: no glow
        }

        static Sprite Droplet(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Disc(m, px, c * 0.5f, c * 0.40f, c * 0.30f);
            TriMask(m, px, new Vector2(c * 0.24f, c * 0.44f), new Vector2(c * 0.76f, c * 0.44f), new Vector2(c * 0.5f, c * 0.96f)); // point up
            return Finish(Shade(m, px, body), px, tex => DrawDot(tex, c * 0.5f, c * 0.40f, c * 0.11f, new Color(0.72f, 0.9f, 1f, 1f)));
        }

        static Sprite Vacuum(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            RoundedRect(m, px, c * 0.52f, c * 0.46f, c * 0.22f, c * 0.40f, c * 0.10f); // tall body
            RoundedRect(m, px, c * 0.26f, c * 0.70f, c * 0.14f, c * 0.07f, c * 0.05f);  // nozzle jutting left
            return Finish(Shade(m, px, body), px, tex =>
            {
                DrawDot(tex, c * 0.56f, c * 0.62f, c * 0.09f, new Color(1f, 0.4f, 0.35f, 1f)); // one angry eye
            });
        }

        static Sprite Sock(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            RoundedRect(m, px, c * 0.5f, c * 0.46f, c * 0.24f, c * 0.36f, c * 0.14f); // a lone tall sock
            return Finish(Shade(m, px, body), px, tex => SmallEyes(tex, px));
        }

        static Sprite Lego(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            RoundedRect(m, px, c * 0.5f, c * 0.46f, c * 0.34f, c * 0.30f, c * 0.06f);
            var stud = new Color(0.82f, 0.82f, 0.9f, 1f);
            return Finish(Shade(m, px, body), px, tex =>
            {
                DrawDot(tex, c * 0.36f, c * 0.66f, c * 0.07f, stud);
                DrawDot(tex, c * 0.64f, c * 0.66f, c * 0.07f, stud);
                DrawDot(tex, c * 0.36f, c * 0.46f, c * 0.07f, stud);
                DrawDot(tex, c * 0.64f, c * 0.46f, c * 0.07f, stud);
            });
        }

        static Sprite Puddle(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Ellipse(m, px, c * 0.5f, c * 0.45f, c * 0.44f, c * 0.28f); // a shallow spill
            return Finish(Shade(m, px, body), px, null);
        }

        static Sprite Cord(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            float headX = 0f, headY = 0f;
            for (int i = 0; i <= 12; i++)
            {
                float t = i / 12f;
                float x = Mathf.Lerp(c * 0.12f, c * 0.88f, t);
                float y = c * 0.5f + Mathf.Sin(t * Mathf.PI * 2f) * c * 0.15f;
                Disc(m, px, x, y, c * 0.095f);
                headX = x; headY = y;
            }
            return Finish(Shade(m, px, body), px, tex =>
            {
                DrawDot(tex, headX - c * 0.03f, headY + c * 0.03f, c * 0.045f, EyeGlow);
                DrawDot(tex, headX + c * 0.03f, headY - c * 0.03f, c * 0.045f, EyeGlow);
            });
        }

        static Sprite MopTrail(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Ellipse(m, px, c * 0.5f, c * 0.5f, c * 0.46f, c * 0.24f); // a soft wet streak, no face
            return Finish(Shade(m, px, body), px, null);
        }

        static Sprite Battery(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            RoundedRect(m, px, c * 0.5f, c * 0.46f, c * 0.24f, c * 0.34f, c * 0.08f); // upright cell
            RoundedRect(m, px, c * 0.5f, c * 0.84f, c * 0.09f, c * 0.05f, c * 0.03f); // top terminal
            return Finish(Shade(m, px, body), px, tex =>
                FillRectTex(tex, px, 0.30f, 0.70f, 0.40f, 0.52f, Palette.Accent)); // charge band
        }

        static Sprite Magnet(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            RoundedRect(m, px, c * 0.30f, c * 0.42f, c * 0.085f, c * 0.26f, c * 0.04f); // left leg
            RoundedRect(m, px, c * 0.70f, c * 0.42f, c * 0.085f, c * 0.26f, c * 0.04f); // right leg
            RoundedRect(m, px, c * 0.50f, c * 0.66f, c * 0.30f, c * 0.085f, c * 0.06f); // joining bar on top
            return Finish(Shade(m, px, body), px, tex =>
            {
                FillRectTex(tex, px, 0.21f, 0.39f, 0.16f, 0.26f, Palette.Accent); // the two pole tips
                FillRectTex(tex, px, 0.61f, 0.79f, 0.16f, 0.26f, Palette.Accent);
            });
        }

        static Sprite Bomb(Color body, int px)
        {
            var m = NewMask(px); float c = px;
            Disc(m, px, c * 0.5f, c * 0.42f, c * 0.32f);                         // round body
            RoundedRect(m, px, c * 0.62f, c * 0.78f, c * 0.04f, c * 0.10f, c * 0.03f); // fuse stub
            return Finish(Shade(m, px, body), px, tex =>
                DrawDot(tex, c * 0.66f, c * 0.90f, c * 0.07f, Palette.Accent)); // spark
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
                float r = baseR * (1f + 0.09f * Mathf.Sin(5f * ang + 0.6f) + 0.05f * Mathf.Sin(8f * ang));
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
