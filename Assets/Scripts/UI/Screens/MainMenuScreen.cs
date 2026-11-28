using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
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

        Button[] menuButtons;
        GameObject previousSelection;

        void Awake()
        {
            instructions.SetActive(false);
            highScores.SetActive(false);
            menuButtons = GetComponentsInChildren<Button>();
        }

        void Start()
        {
            if (menuButtons.Length > 0) menuButtons[0].Select();
        }

        public void PlayGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Game");
        }

        public void ShowInstructions() => OpenPanel(instructions);
        public void CloseInstructions() => ClosePanel(instructions);
        public void CloseHighScores() => ClosePanel(highScores);
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
            OpenPanel(highScores);
        }

        void OpenPanel(GameObject panel)
        {
            var events = EventSystem.current;
            previousSelection = events != null ? events.currentSelectedGameObject : null;
            foreach (var button in menuButtons) button.interactable = false;
            panel.SetActive(true);
            PanelEntrance.Play(panel);
            panel.GetComponentInChildren<Button>().Select();
        }

        void ClosePanel(GameObject panel)
        {
            panel.SetActive(false);
            foreach (var button in menuButtons) button.interactable = true;
            if (EventSystem.current != null && previousSelection != null)
                EventSystem.current.SetSelectedGameObject(previousSelection);
        }
    }
}
