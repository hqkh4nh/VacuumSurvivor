using UnityEngine;

namespace VacuumSurvivor
{
    // Uses real time so panels can arrive while gameplay is paused.
    internal sealed class PanelEntrance : MonoBehaviour
    {
        CanvasGroup group;
        Vector3 restScale;
        float elapsed;
        float duration;

        public static void Play(GameObject panel, float seconds = 0.2f)
        {
            if (!Application.isPlaying) return;
            var entrance = panel.GetComponent<PanelEntrance>();
            if (entrance == null) entrance = panel.AddComponent<PanelEntrance>();
            entrance.Restore();
            entrance.restScale = panel.transform.localScale;
            entrance.elapsed = 0f;
            entrance.duration = seconds;
            entrance.enabled = true;
            entrance.Apply(0f);
        }

        void Awake()
        {
            group = gameObject.AddComponent<CanvasGroup>();
            restScale = transform.localScale;
        }

        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            Apply(1f - Mathf.Pow(1f - progress, 3f));
            if (progress >= 1f) enabled = false;
        }

        void Apply(float progress)
        {
            group.alpha = Mathf.Lerp(0.45f, 1f, progress);
            transform.localScale = restScale * Mathf.Lerp(0.965f, 1f, progress);
        }

        void OnDisable() => Restore();

        void Restore()
        {
            if (group == null) return;
            group.alpha = 1f;
            transform.localScale = restScale;
        }
    }
}
