using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// The main-menu scene UI: title and the Chơi / Hướng dẫn / Điểm cao / Thoát buttons, plus the
    /// instructions and high-score panels they toggle. Built in code with legacy Text so Vietnamese
    /// diacritics render without a font asset.
    public class MainMenuScreen : MonoBehaviour
    {
        GameObject instructions;
        GameObject highScores;
        Text highScoreText;

        void Awake() => Build();

        void Build()
        {
            UIFactory.Stretch(gameObject);
            UIFactory.Panel(transform, new Color(0.05f, 0.05f, 0.1f, 1f), "Bg");

            var title = UIFactory.Label(transform, "VACUUM SURVIVOR", 56, TextAnchor.MiddleCenter, new Color(0.91f, 0.91f, 0.94f));
            var tr = title.rectTransform;
            tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0.5f, 0.5f);
            tr.sizeDelta = new Vector2(900f, 120f);
            tr.anchoredPosition = new Vector2(0f, 200f);

            MakeButton("Chơi", new Vector2(0f, 60f), () => SceneManager.LoadScene("Game"));
            MakeButton("Hướng dẫn", new Vector2(0f, -20f), () => Toggle(instructions));
            MakeButton("Điểm cao", new Vector2(0f, -100f), ShowHighScores);
            MakeButton("Thoát", new Vector2(0f, -180f), Application.Quit);

            instructions = BuildInfoPanel(
                "WASD / phím mũi tên để di chuyển.\n" +
                "Vũ khí tự đánh — chỉ cần né rác và nhặt bụi.\n" +
                "Pin tụt dần; đứng lên ụ sạc để hồi pin.\n" +
                "Nhặt đủ bụi thì lên cấp và chọn nâng cấp.\n" +
                "Dọn sạch 3 phòng và hạ boss cuối để thắng.");
            highScores = BuildInfoPanel("");
            highScoreText = highScores.GetComponentInChildren<Text>();
        }

        void ShowHighScores()
        {
            var entries = SaveSystem.Load();
            var sb = new StringBuilder("ĐIỂM CAO\n\n");
            if (entries.Count == 0) sb.Append("Chưa có điểm nào.");
            else
                for (int i = 0; i < entries.Count; i++)
                    sb.Append($"{i + 1}. {entries[i].score} điểm · Cấp {entries[i].level} · {entries[i].date}\n");
            highScoreText.text = sb.ToString();
            Toggle(highScores);
        }

        static void Toggle(GameObject panel) => panel.SetActive(!panel.activeSelf);

        GameObject BuildInfoPanel(string text)
        {
            var panel = UIFactory.Panel(transform, new Color(0.02f, 0.02f, 0.05f, 0.95f), "Info").gameObject;
            var label = UIFactory.Label(panel.transform, text, 26, TextAnchor.MiddleCenter, new Color(0.88f, 0.88f, 0.95f));
            var lr = label.rectTransform;
            lr.anchorMin = lr.anchorMax = lr.pivot = new Vector2(0.5f, 0.5f);
            lr.sizeDelta = new Vector2(820f, 360f);
            lr.anchoredPosition = new Vector2(0f, 40f);

            var close = UIFactory.Button(panel.transform, new Color(0.42f, 0.48f, 0.6f, 1f), "Close");
            var cr = close.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(200f, 56f);
            cr.anchoredPosition = new Vector2(0f, -200f);
            close.onClick.AddListener(() => panel.SetActive(false));
            var cl = UIFactory.Label(close.transform, "Đóng", 24, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(cl.gameObject);

            panel.SetActive(false);
            return panel;
        }

        void MakeButton(string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var btn = UIFactory.Button(transform, new Color(0.29f, 0.87f, 0.5f, 1f), text);
            var br = btn.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(300f, 64f);
            br.anchoredPosition = pos;
            btn.onClick.AddListener(onClick);
            var label = UIFactory.Label(btn.transform, text, 28, TextAnchor.MiddleCenter, new Color(0.05f, 0.1f, 0.05f));
            UIFactory.Stretch(label.gameObject);
        }
    }
}
