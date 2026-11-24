using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    internal sealed class GameplayOverlay : MonoBehaviour
    {
        static readonly Color Mint = new Color(0.59f, 0.98f, 0.83f);
        static readonly Color Panel = new Color(0.075f, 0.14f, 0.16f, 0.98f);
        const string VolumeKey = "VacuumSurvivor.MasterVolume";

        RectTransform hud;
        Font font;
        Sprite frame;
        GameObject pause;
        RectTransform pauseCard;
        Button resume;
        RectTransform marker;
        RectTransform arrow;
        Image dockIcon;
        DockSpawner docks;
        Camera worldCamera;

        public void Configure(Font textFont, Sprite panelFrame)
        {
            hud = (RectTransform)transform;
            font = textFont;
            frame = panelFrame;
            worldCamera = Camera.main;
            docks = FindAnyObjectByType<DockSpawner>();

            marker = ImageAt(hud, "Dock indicator", Vector2.zero, new Vector2(98f, 52f), Panel, true).rectTransform;
            dockIcon = ImageAt(marker, "Dock icon", new Vector2(-23f, 0f), new Vector2(34f, 34f), Color.white);
            dockIcon.preserveAspect = true;
            Label(marker, "Ụ SẠC", new Vector2(18f, 0f), new Vector2(50f, 26f), 13, Mint);
            arrow = Label(marker, "▲", Vector2.zero, new Vector2(28f, 28f), 22, Mint).rectTransform;
            marker.gameObject.SetActive(false);

            var shade = ImageAt(hud, "Pause overlay", Vector2.zero, Vector2.zero, new Color(0.015f, 0.034f, 0.047f, 0.79f));
            shade.raycastTarget = true;
            shade.rectTransform.anchorMin = Vector2.zero;
            shade.rectTransform.anchorMax = Vector2.one;
            pause = shade.gameObject;
            pauseCard = ImageAt(shade.rectTransform, "Pause card", Vector2.zero, new Vector2(388f, 292f), Panel, true).rectTransform;
            Label(pauseCard, "TẠM DỪNG", new Vector2(0f, 97f), new Vector2(320f, 40f), 28, Mint);

            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
            Label(pauseCard, "Âm lượng", new Vector2(-85f, 43f), new Vector2(136f, 24f), 18, Color.white);
            BuildSlider();

            resume = MakeButton("Tiếp tục", new Vector2(0f, -21f), Mint, Panel);
            resume.onClick.AddListener(() => GameManager.Instance.TogglePause());
            MakeButton("Về menu", new Vector2(0f, -78f), new Color(0.19f, 0.31f, 0.32f), Color.white)
                .onClick.AddListener(ReturnToMenu);
            pause.SetActive(false);
        }

        void BuildSlider()
        {
            var track = ImageAt(pauseCard, "Volume slider", new Vector2(57f, 43f), new Vector2(164f, 32f), Panel);
            track.raycastTarget = true;
            ImageAt(track.rectTransform, "Track", Vector2.zero, new Vector2(156f, 6f), new Color(0.25f, 0.39f, 0.4f));
            var handleArea = ImageAt(track.rectTransform, "Handle area", Vector2.zero, Vector2.zero, Color.clear).rectTransform;
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.sizeDelta = new Vector2(-16f, 0f);
            var handle = ImageAt(handleArea, "Thumb", Vector2.zero, new Vector2(16f, 23f), Mint);
            var slider = track.gameObject.AddComponent<Slider>();
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.value = AudioListener.volume;
            slider.onValueChanged.AddListener(value =>
            {
                AudioListener.volume = value;
                PlayerPrefs.SetFloat(VolumeKey, value);
            });
        }

        Button MakeButton(string title, Vector2 position, Color background, Color foreground)
        {
            var image = ImageAt(pauseCard, title, position, new Vector2(294f, 44f), background, true);
            image.raycastTarget = true;
            Label(image.rectTransform, title, Vector2.zero, new Vector2(280f, 36f), 20, foreground);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        void ReturnToMenu()
        {
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        void LateUpdate()
        {
            if (pause == null) return;
            var game = GameManager.Instance;
            bool paused = game != null && game.Current == GameManager.State.Paused;
            if (pause.activeSelf != paused)
            {
                pause.SetActive(paused);
                if (paused)
                {
                    PanelEntrance.Play(pauseCard.gameObject);
                    resume.Select();
                }
                else
                {
                    if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null &&
                        EventSystem.current.currentSelectedGameObject.transform.IsChildOf(pauseCard))
                        EventSystem.current.SetSelectedGameObject(null);
                    PlayerPrefs.Save();
                }
            }

            Transform dock = docks != null ? docks.ActiveDock : null;
            bool canShow = game != null && game.Current == GameManager.State.Playing && Time.timeScale > 0f &&
                dock != null && worldCamera != null;
            if (canShow) canShow = PlaceMarker(dock);
            if (marker.gameObject.activeSelf != canShow) marker.gameObject.SetActive(canShow);
        }

        bool PlaceMarker(Transform dock)
        {
            var view = worldCamera.WorldToViewportPoint(dock.position);
            if (view.x >= 0f && view.x <= 1f && view.y >= 0f && view.y <= 1f) return false;

            Vector2 direction = Vector2.Scale((Vector2)view - Vector2.one * 0.5f, hud.rect.size);
            Vector2 edge = Vector2.Max(hud.rect.size * 0.5f - new Vector2(95f, 100f), Vector2.one);
            float scale = Mathf.Max(Mathf.Abs(direction.x) / edge.x, Mathf.Abs(direction.y) / edge.y);
            marker.anchoredPosition = direction / scale;
            Vector2 tip = direction.normalized;
            arrow.anchoredPosition = tip * 40f;
            arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tip.y, tip.x) * Mathf.Rad2Deg - 90f);
            if (dock.TryGetComponent<SpriteRenderer>(out var renderer))
                dockIcon.sprite = renderer.sprite;
            return true;
        }

        Image ImageAt(RectTransform parent, string name, Vector2 position, Vector2 size, Color color, bool framed = false)
        {
            var rect = Place(parent, name, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (framed && frame != null)
            {
                image.sprite = frame;
                image.type = Image.Type.Sliced;
            }
            return image;
        }

        Text Label(RectTransform parent, string text, Vector2 position, Vector2 size, int pointSize, Color color)
        {
            var label = Place(parent, text, position, size).gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = pointSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        static RectTransform Place(RectTransform parent, string name, Vector2 position, Vector2 size)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.layer = parent.gameObject.layer;
            var rect = (RectTransform)child.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
