using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// Shown on win or loss: the outcome message, this run's stats and score, and buttons to play
    /// again or return to the main menu.
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Text message;
        [SerializeField] Text stats;

        void Awake() => root.SetActive(false);

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
            PanelEntrance.Play(root, 0.28f);
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

        public void PlayAgain() => GameManager.Restart();

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
