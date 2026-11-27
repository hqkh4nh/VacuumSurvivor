using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    public sealed class GameplayOverlay : MonoBehaviour
    {
        const string VolumeKey = "VacuumSurvivor.MasterVolume";

        [SerializeField] GameObject pause;
        [SerializeField] RectTransform pauseCard;
        [SerializeField] Button resume;
        [SerializeField] Slider volume;
        [SerializeField] RectTransform marker;
        [SerializeField] RectTransform arrow;
        [SerializeField] Image dockIcon;

        RectTransform hud;
        DockSpawner docks;
        Camera worldCamera;

        void Awake()
        {
            hud = (RectTransform)transform;
            worldCamera = Camera.main;
            docks = FindAnyObjectByType<DockSpawner>();
            pause.SetActive(false);
            marker.gameObject.SetActive(false);
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
            volume.SetValueWithoutNotify(AudioListener.volume);
        }

        public void Resume()
        {
            var game = GameManager.Instance;
            if (game != null && game.Current == GameManager.State.Paused) game.TogglePause();
        }

        public void SetVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
        }

        public void ReturnToMenu()
        {
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        void LateUpdate()
        {
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
    }
}
