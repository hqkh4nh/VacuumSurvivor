using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// Small helpers for building legacy uGUI at runtime. Legacy Text with the built-in font keeps
    /// Vietnamese diacritics readable without authoring a TextMeshPro font asset.
    public static class UIFactory
    {
        static Font cached;
        public static Font Font => cached != null ? cached : (cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public static RectTransform Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(Transform parent, Color color, string name = "Panel")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch(go);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        public static Image Box(Transform parent, Color color, string name = "Box")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<Image>();
        }

        public static Text Label(Transform parent, string text, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// A top-centre boss health bar: dark background, red fill that shrinks from the right
        /// (via the fill's anchorMax.x, like the HUD's other bars) and a label above it.
        public static GameObject BossBar(Transform parent, out Image fill, out Text label)
        {
            var root = new GameObject("BossBar", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(520f, 24f);
            rt.anchoredPosition = new Vector2(0f, -56f);

            var bg = Box(root.transform, new Color(0f, 0f, 0f, 0.6f), "BarBg");
            Stretch(bg.gameObject);

            fill = Box(bg.transform, new Color(0.85f, 0.25f, 0.3f), "Fill");
            var fr = fill.rectTransform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(2f, 2f);
            fr.offsetMax = new Vector2(-2f, -2f);

            label = Label(root.transform, "CON MÈO", 16, TextAnchor.LowerCenter, Color.white);
            var lr = label.rectTransform;
            lr.anchorMin = lr.anchorMax = lr.pivot = new Vector2(0.5f, 0f);
            lr.sizeDelta = new Vector2(520f, 24f);
            lr.anchoredPosition = new Vector2(0f, 2f);
            return root;
        }

        public static Button Button(Transform parent, Color bg, string name = "Button")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;
            return go.GetComponent<Button>();
        }
    }
}
