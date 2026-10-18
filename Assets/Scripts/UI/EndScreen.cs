using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// Shown on win or loss: a message and a "Chơi lại" button that restarts the slice.
    public class EndScreen : MonoBehaviour
    {
        GameObject root;
        Text message;

        void Awake()
        {
            UIFactory.Stretch(gameObject);
            Build();
            root.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.Won += OnWon;
            GameEvents.Lost += OnLost;
        }

        void OnDisable()
        {
            GameEvents.Won -= OnWon;
            GameEvents.Lost -= OnLost;
        }

        void OnWon() => Show("NHÀ SẠCH BONG!\nDọn xong lát này.", new Color(0.29f, 0.87f, 0.5f));
        void OnLost() => Show("Chủ về nhà và thấy\nmột đống bừa bộn.", new Color(0.937f, 0.267f, 0.267f));

        void Show(string text, Color color)
        {
            message.text = text;
            message.color = color;
            root.SetActive(true);
        }

        void Build()
        {
            root = UIFactory.Panel(transform, new Color(0.03f, 0.03f, 0.07f, 0.92f), "Root").gameObject;

            message = UIFactory.Label(root.transform, "", 44, TextAnchor.MiddleCenter, Color.white);
            var mr = message.rectTransform;
            mr.anchorMin = mr.anchorMax = mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(900f, 200f);
            mr.anchoredPosition = new Vector2(0f, 70f);

            var btn = UIFactory.Button(root.transform, new Color(0.29f, 0.87f, 0.5f, 1f), "Replay");
            var br = btn.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(240f, 64f);
            br.anchoredPosition = new Vector2(0f, -90f);
            btn.onClick.AddListener(GameManager.Restart);

            var label = UIFactory.Label(btn.transform, "Chơi lại", 26, TextAnchor.MiddleCenter, new Color(0.05f, 0.1f, 0.05f));
            UIFactory.Stretch(label.gameObject);
        }
    }
}
