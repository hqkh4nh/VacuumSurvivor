using UnityEngine;
using UnityEngine.EventSystems;

namespace VacuumSurvivor
{
    /// Moves the authored card face while keeping its clickable rectangle stationary.
    [DisallowMultipleComponent]
    public sealed class UpgradeCardFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] RectTransform visual;
        [SerializeField] GameObject highlightBorder;
        [SerializeField] float lift = 6f;
        [SerializeField] float response = 12f;

        Vector2 restPosition;
        Vector3 restScale;
        bool ready, hovered, selected, pressed;
        float highlight, depression;

        void Awake()
        {
            restPosition = visual.anchoredPosition;
            restScale = visual.localScale;
            ready = true;
        }

        void Update()
        {
            if (highlightBorder != null) highlightBorder.SetActive(hovered || selected);
            highlight = Mathf.MoveTowards(highlight, hovered || selected ? 1f : 0f, Time.unscaledDeltaTime * response);
            depression = Mathf.MoveTowards(depression, pressed ? 1f : 0f, Time.unscaledDeltaTime * response);
            visual.anchoredPosition = restPosition + Vector2.up * (lift * highlight);
            visual.localScale = restScale * Mathf.Lerp(1f, 0.985f, depression);
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData eventData) { if (eventData.button == PointerEventData.InputButton.Left) pressed = true; }
        public void OnPointerUp(PointerEventData eventData) => pressed = false;
        public void OnSelect(BaseEventData eventData) => selected = true;
        public void OnDeselect(BaseEventData eventData) { selected = false; pressed = false; }

        void OnDisable()
        {
            hovered = selected = pressed = false;
            highlight = depression = 0f;
            if (highlightBorder != null) highlightBorder.SetActive(false);
            if (!ready) return;
            visual.anchoredPosition = restPosition;
            visual.localScale = restScale;
        }
    }
}
