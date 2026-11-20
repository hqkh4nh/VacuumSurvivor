using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// A centre-screen banner that appears during the between-level transition announcing the room
    /// the robot just cleared.
    public class LevelBanner : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Text label;
        string clearedRoom = "";
        string currentRoom = "";
        bool showing;

        void Awake() => root.SetActive(false);

        void OnEnable() => GameEvents.LevelChanged += OnLevelChanged;
        void OnDisable() => GameEvents.LevelChanged -= OnLevelChanged;

        void OnLevelChanged(int index, string name)
        {
            clearedRoom = currentRoom; // the room finished a moment ago
            currentRoom = name;
        }

        void Update()
        {
            bool shouldShow = GameManager.Instance != null &&
                              GameManager.Instance.Current == GameManager.State.Transition;
            if (shouldShow && !showing)
            {
                label.text = $"Đã dọn sạch {clearedRoom}!";
                root.SetActive(true);
                showing = true;
            }
            else if (!shouldShow && showing)
            {
                root.SetActive(false);
                showing = false;
            }
        }
    }
}
