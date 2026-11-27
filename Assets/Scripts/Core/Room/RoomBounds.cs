using UnityEngine;

namespace VacuumSurvivor
{
    /// Owns the playable rectangle and selects existing room prefabs.
    public class RoomBounds : MonoBehaviour
    {
        public static RoomBounds Instance { get; private set; }

        [SerializeField] Vector2 size = new Vector2(32f, 24f);
        [SerializeField] float wallThickness = 0.5f;
        [SerializeField] RoomLayout[] layouts;
        [SerializeField] int selectedLayout;

        public RoomLayout ActiveLayout => layouts != null && layouts.Length > 0 ? layouts[selectedLayout] : null;
        public Vector2 Size => ActiveLayout != null ? ActiveLayout.Size : size;

        public void SelectLayout(int index)
        {
            if (layouts == null || layouts.Length == 0) return;
            selectedLayout = Mathf.Clamp(index, 0, layouts.Length - 1);
            for (int i = 0; i < layouts.Length; i++) layouts[i].gameObject.SetActive(i == selectedLayout);
            Physics2D.SyncTransforms();
        }

        Vector2 Scale => new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        public Rect Area => new Rect((Vector2)transform.position - Vector2.Scale(Size, Scale) * 0.5f, Vector2.Scale(Size, Scale));
        public Rect OuterArea
        {
            get
            {
                Vector2 outerSize = Vector2.Scale(Size + Vector2.one * (wallThickness * 2f), Scale);
                return new Rect((Vector2)transform.position - outerSize * 0.5f, outerSize);
            }
        }

        void Awake() { Instance = this; SelectLayout(0); }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public Vector2 Clamp(Vector2 point, float radius = 0f) => RoomGeometry.Clamp(Area, point, Vector2.one * radius);

        public Vector2 ClearPosition(Vector2 point, float radius = 0f)
        {
            var position = Clamp(point, radius);
            return ActiveLayout != null ? ActiveLayout.ClearPosition(position, radius, Area) : position;
        }

        public static float RadiusOf(GameObject body)
        {
            var circle = body.GetComponent<CircleCollider2D>();
            if (circle == null) return 0f;
            var scale = body.transform.lossyScale;
            return circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        }

        /// Clamps teleports immediately and limits the next physics step to the playable area.
        public void Constrain(Rigidbody2D body)
        {
            float radius = RadiusOf(body.gameObject);
            Vector2 position = Clamp(body.position, radius);
            Vector2 velocity = body.linearVelocity;
            Vector2 requested = position + velocity * Time.fixedDeltaTime;
            Vector2 next = Clamp(requested, radius);
            if (body.position != position) body.position = position;
            if (next.x != requested.x) velocity.x = (next.x - position.x) / Time.fixedDeltaTime;
            if (next.y != requested.y) velocity.y = (next.y - position.y) / Time.fixedDeltaTime;
            if (velocity != body.linearVelocity) body.linearVelocity = velocity;
        }
    }
}
