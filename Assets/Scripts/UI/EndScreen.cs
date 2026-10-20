using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// Shown on win or loss: the outcome message, this run's stats and score, and buttons to play
    /// again or return to the main menu.
    public class EndScreen : MonoBehaviour
    {
        GameObject root;
        Text message;
        Text stats;

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

        void OnWon() => Show("NHÀ SẠCH BONG!\nDọn xong lát này.", new Color(0.29f, 0.87f, 0.5f), true);
        void OnLost() => Show("Chủ về nhà và thấy\nmột đống bừa bộn.", new Color(0.937f, 0.267f, 0.267f), false);

        void Show(string text, Color color, bool won)
        {
            message.text = text;
            message.color = color;
            stats.text = Summary(won);
            root.SetActive(true);
        }

        static string Summary(bool won)
        {
            var t = RunTracker.Instance;
            if (t == null) return "";
            var s = t.Last;
            return $"Thời gian: {s.seconds}s    Rác dọn: {s.kills}\n" +
                   $"Bụi: {s.dust}    Cấp: {t.Level}\n" +
                   $"Điểm: {s.Score(won)}";
        }

        void Build()
        {
            root = UIFactory.Panel(transform, new Color(0.03f, 0.03f, 0.07f, 0.92f), "Root").gameObject;

            message = UIFactory.Label(root.transform, "", 44, TextAnchor.MiddleCenter, Color.white);
            var mr = message.rectTransform;
            mr.anchorMin = mr.anchorMax = mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(900f, 180f);
            mr.anchoredPosition = new Vector2(0f, 150f);

            stats = UIFactory.Label(root.transform, "", 26, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.95f));
            var sr = stats.rectTransform;
            sr.anchorMin = sr.anchorMax = sr.pivot = new Vector2(0.5f, 0.5f);
            sr.sizeDelta = new Vector2(900f, 160f);
            sr.anchoredPosition = new Vector2(0f, -20f);

            MakeButton("Chơi lại", new Color(0.29f, 0.87f, 0.5f, 1f), new Vector2(-140f, -150f), GameManager.Restart);
            MakeButton("Về menu", new Color(0.42f, 0.48f, 0.6f, 1f), new Vector2(140f, -150f),
                () => SceneManager.LoadScene("MainMenu"));
        }

        void MakeButton(string text, Color color, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.Button(root.transform, color, text);
            var br = btn.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(240f, 64f);
            br.anchoredPosition = pos;
            btn.onClick.AddListener(onClick);

            var label = UIFactory.Label(btn.transform, text, 26, TextAnchor.MiddleCenter, new Color(0.05f, 0.1f, 0.05f));
            UIFactory.Stretch(label.gameObject);
        }
    }
}
