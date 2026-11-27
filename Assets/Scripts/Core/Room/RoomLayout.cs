using UnityEngine;

namespace VacuumSurvivor
{
    /// Authored room art and collision references; furniture is positioned in the prefab.
    public sealed class RoomLayout : MonoBehaviour
    {
        [SerializeField] Vector2 size;
        [SerializeField] SpriteRenderer floor;
        [SerializeField] BoxCollider2D[] walls;
        [SerializeField] BoxCollider2D[] obstacles;

        public Vector2 Size => size;
        public SpriteRenderer Floor => floor;

        public Vector2 ClearPosition(Vector2 point, float radius, Rect area)
        {
            float padding = radius + 0.16f;
            bool Blocked(Vector2 candidate)
            {
                foreach (var obstacle in obstacles)
                {
                    var bounds = obstacle.bounds;
                    if (candidate.x > bounds.min.x - padding && candidate.x < bounds.max.x + padding &&
                        candidate.y > bounds.min.y - padding && candidate.y < bounds.max.y + padding) return true;
                }
                return false;
            }
            if (!Blocked(point)) return point;
            Vector2 best = area.center;
            float distance = float.PositiveInfinity;
            void Consider(Vector2 candidate)
            {
                candidate = RoomGeometry.Clamp(area, candidate, Vector2.one * radius);
                float next = (candidate - point).sqrMagnitude;
                if (next >= distance || Blocked(candidate)) return;
                best = candidate;
                distance = next;
            }
            Consider(area.center);
            foreach (var obstacle in obstacles)
            {
                var bounds = obstacle.bounds;
                Consider(new Vector2(bounds.min.x - padding, point.y));
                Consider(new Vector2(bounds.max.x + padding, point.y));
                Consider(new Vector2(point.x, bounds.min.y - padding));
                Consider(new Vector2(point.x, bounds.max.y + padding));
            }
            return best;
        }
    }
}
