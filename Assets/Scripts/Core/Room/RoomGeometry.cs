using UnityEngine;

namespace VacuumSurvivor
{
    /// Shared room math for body positions, camera viewports and enemy spawn selection.
    public static class RoomGeometry
    {
        public static Vector2 Clamp(Rect room, Vector2 position, Vector2 halfSize)
        {
            return new Vector2(
                ClampAxis(position.x, room.xMin, room.xMax, halfSize.x),
                ClampAxis(position.y, room.yMin, room.yMax, halfSize.y));
        }

        static float ClampAxis(float value, float min, float max, float margin)
        {
            margin = Mathf.Max(0f, margin);
            return margin * 2f >= max - min ? (min + max) * 0.5f : Mathf.Clamp(value, min + margin, max - margin);
        }

        public static Vector2 Spawn(Rect room, Vector2 player, Rect view, float radius, float distance, float angle)
        {
            float padding = radius + 0.25f;
            Rect visible = Rect.MinMaxRect(view.xMin - padding, view.yMin - padding, view.xMax + padding, view.yMax + padding);
            Vector2 margin = Vector2.one * radius;
            float minDistance = Mathf.Min(3f, distance);
            for (int i = 0; i < 32; i++)
            {
                float a = (angle + i * 360f / 32f) * Mathf.Deg2Rad;
                Vector2 candidate = Clamp(room, player + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * distance, margin);
                if (!visible.Contains(candidate) && Vector2.Distance(candidate, player) >= minDistance) return candidate;
            }

            // Near a wall the spawn ring can be entirely visible, while the opposite side of the
            // room is hidden. Try the viewport edges before falling back to the room's corners.
            var candidates = new[]
            {
                new Vector2(player.x, visible.yMin - 0.01f), new Vector2(player.x, visible.yMax + 0.01f),
                new Vector2(visible.xMin - 0.01f, player.y), new Vector2(visible.xMax + 0.01f, player.y),
                new Vector2(room.xMin, room.yMin), new Vector2(room.xMax, room.yMin),
                new Vector2(room.xMin, room.yMax), new Vector2(room.xMax, room.yMax),
            };
            Vector2 nearestHidden = Vector2.zero;
            Vector2 farthest = Clamp(room, player, margin);
            float nearestDistance = float.PositiveInfinity;
            float farthestDistance = -1f;
            foreach (var point in candidates)
            {
                Vector2 candidate = Clamp(room, point, margin);
                float d = Vector2.Distance(player, candidate);
                if (d > farthestDistance) { farthestDistance = d; farthest = candidate; }
                if (!visible.Contains(candidate) && d >= minDistance && d < nearestDistance)
                { nearestDistance = d; nearestHidden = candidate; }
            }
            return float.IsPositiveInfinity(nearestDistance) ? farthest : nearestHidden;
        }
    }
}
