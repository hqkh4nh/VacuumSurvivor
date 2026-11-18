using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// Controls the authored menu Canvas and displays the saved high-score board.
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] GameObject instructions;
        [SerializeField] GameObject highScores;
        [SerializeField] Text highScoreText;

        void Awake()
        {
            instructions.SetActive(false);
            highScores.SetActive(false);
        }

        public void PlayGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Game");
        }

        public void ShowInstructions() => instructions.SetActive(true);
        public void CloseInstructions() => instructions.SetActive(false);
        public void CloseHighScores() => highScores.SetActive(false);
        public void QuitGame() => Application.Quit();

        public void ShowHighScores()
        {
            var entries = SaveSystem.Load();
            var sb = new StringBuilder("ĐIỂM CAO\n\n");
            if (entries.Count == 0) sb.Append("Chưa có điểm nào.");
            else
                for (int i = 0; i < entries.Count; i++)
                    sb.Append($"{i + 1}. {entries[i].score} điểm · Cấp {entries[i].level} · {entries[i].date}\n");
            highScoreText.text = sb.ToString();
            highScores.SetActive(true);
        }

    }
}
