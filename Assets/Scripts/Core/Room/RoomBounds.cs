using UnityEngine;

namespace VacuumSurvivor
{
    /// Owns the room's playable rectangle; the existing floor and walls follow its Inspector size.
    public class RoomBounds : MonoBehaviour
    {
        public static RoomBounds Instance { get; private set; }

        [SerializeField] Vector2 size = new Vector2(32f, 24f);
        [SerializeField] float wallThickness = 0.5f;

        public Vector2 Size => size;
        Vector2 Scale => new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        public Rect Area => new Rect((Vector2)transform.position - Vector2.Scale(size, Scale) * 0.5f, Vector2.Scale(size, Scale));
        public Rect OuterArea
        {
            get
            {
                Vector2 outerSize = Vector2.Scale(size + Vector2.one * (wallThickness * 2f), Scale);
                return new Rect((Vector2)transform.position - outerSize * 0.5f, outerSize);
            }
        }

        void Awake() => Instance = this;
        void OnDestroy() { if (Instance == this) Instance = null; }

        void OnValidate()
        {
            size = Vector2.Max(size, Vector2.one * 4f);
            wallThickness = Mathf.Max(0.1f, wallThickness);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= RefreshLayout;
            UnityEditor.EditorApplication.delayCall += RefreshLayout;
#endif
        }

        /// Resizes the existing floor and four walls to match the room settings.
        public void RefreshLayout()
        {
            if (this == null) return;
            var floor = GetComponent<SpriteRenderer>();
            if (floor != null) floor.size = size;
            FitWall("NorthWall", new Vector2(0f, (size.y + wallThickness) * 0.5f), new Vector2(size.x, wallThickness));
            FitWall("SouthWall", new Vector2(0f, -(size.y + wallThickness) * 0.5f), new Vector2(size.x, wallThickness));
            FitWall("EastWall", new Vector2((size.x + wallThickness) * 0.5f, 0f), new Vector2(wallThickness, size.y + wallThickness * 2f));
            FitWall("WestWall", new Vector2(-(size.x + wallThickness) * 0.5f, 0f), new Vector2(wallThickness, size.y + wallThickness * 2f));
        }

        void FitWall(string name, Vector2 position, Vector2 dimensions)
        {
            var wall = transform.Find(name);
            if (wall == null) return;
            wall.localPosition = position;
            wall.GetComponent<BoxCollider2D>().size = dimensions;
            wall.GetComponent<SpriteRenderer>().size = dimensions;
        }

        public Vector2 Clamp(Vector2 point, float radius = 0f) => RoomGeometry.Clamp(Area, point, Vector2.one * radius);

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
