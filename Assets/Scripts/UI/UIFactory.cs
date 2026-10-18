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

        public static Button Button(Transform parent, Color bg, string name = "Button")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;
            return go.GetComponent<Button>();
        }
    }
}
