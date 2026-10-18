using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// The three-card upgrade chooser. Builds itself, freezes the game while open, and reports the
    /// picked index. Uses unscaled time so it works while the game is paused.
    public class LevelUpPanel : MonoBehaviour
    {
        GameObject root;
        readonly Text[] titles = new Text[3];
        readonly Text[] descs = new Text[3];
        readonly Button[] buttons = new Button[3];
        Action<int> onPick;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            UIFactory.Stretch(gameObject);
            Build();
            root.SetActive(false);
        }

        public void Show(IReadOnlyList<UpgradeData> options, Action<int> pick)
        {
            onPick = pick;
            for (int i = 0; i < 3; i++)
            {
                bool has = i < options.Count;
                buttons[i].gameObject.SetActive(has);
                if (!has) continue;
                titles[i].text = options[i].title;
                descs[i].text = options[i].description;
            }
            root.SetActive(true);
            Time.timeScale = 0f;
        }

        void Choose(int index)
        {
            root.SetActive(false);
            var callback = onPick;
            onPick = null;
            callback?.Invoke(index);
        }

        void Build()
        {
            root = UIFactory.Panel(transform, new Color(0.04f, 0.04f, 0.09f, 0.9f), "Root").gameObject;

            var title = UIFactory.Label(root.transform, "LÊN CẤP — CHỌN 1 NÂNG CẤP", 34, TextAnchor.UpperCenter, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.sizeDelta = new Vector2(900f, 60f);
            trt.anchoredPosition = new Vector2(0f, -50f);

            float[] xs = { -340f, 0f, 340f };
            for (int i = 0; i < 3; i++)
            {
                var btn = UIFactory.Button(root.transform, new Color(0.18f, 0.18f, 0.30f, 1f), "Card");
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.5f);
                brt.sizeDelta = new Vector2(300f, 360f);
                brt.anchoredPosition = new Vector2(xs[i], -10f);
                int index = i;
                btn.onClick.AddListener(() => Choose(index));

                var t = UIFactory.Label(btn.transform, "", 24, TextAnchor.UpperCenter, Color.white);
                var tr = t.rectTransform;
                tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f);
                tr.offsetMin = new Vector2(12f, -80f); tr.offsetMax = new Vector2(-12f, -14f);
                titles[i] = t;

                var d = UIFactory.Label(btn.transform, "", 17, TextAnchor.UpperLeft, new Color(0.82f, 0.82f, 0.88f));
                var dr = d.rectTransform;
                dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
                dr.offsetMin = new Vector2(14f, 14f); dr.offsetMax = new Vector2(-14f, -90f);
                descs[i] = d;

                buttons[i] = btn;
            }
        }
    }
}
