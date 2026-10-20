using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// A centre-screen banner that appears during the between-level transition announcing the room
    /// the robot just cleared.
    public class LevelBanner : MonoBehaviour
    {
        GameObject root;
        Text label;
        string clearedRoom = "";
        string currentRoom = "";
        bool showing;

        void Awake()
        {
            UIFactory.Stretch(gameObject);
            root = UIFactory.Panel(transform, new Color(0.03f, 0.03f, 0.07f, 0.75f), "Root").gameObject;
            label = UIFactory.Label(root.transform, "", 44, TextAnchor.MiddleCenter, new Color(0.29f, 0.87f, 0.5f));
            var lr = label.rectTransform;
            lr.anchorMin = lr.anchorMax = lr.pivot = new Vector2(0.5f, 0.5f);
            lr.sizeDelta = new Vector2(900f, 160f);
            root.SetActive(false);
        }

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
