using UnityEngine;

namespace VacuumSurvivor
{
    /// Smoothly tracks a target in the XY plane, keeping the camera's own depth.
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smooth = 8f;

        Camera view;
        float preferredSize;

        public Transform Target { get => target; set => target = value; }

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 goal = target.position;
            goal.z = transform.position.z;
            var room = RoomBounds.Instance;
            if (view == null)
            {
                view = GetComponent<Camera>();
                preferredSize = view.orthographicSize;
            }
            if (room != null)
                view.orthographicSize = Mathf.Min(preferredSize, room.OuterArea.height * 0.5f, room.OuterArea.width / (2f * view.aspect));
            Vector2 halfView = new Vector2(view.orthographicSize * view.aspect, view.orthographicSize);
            if (room != null)
            {
                Vector2 center = RoomGeometry.Clamp(room.OuterArea, goal, halfView);
                goal.x = center.x; goal.y = center.y;
            }
            // Exponential smoothing: frame-rate independent, no overshoot.
            Vector3 next = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            if (room != null)
            {
                Vector2 center = RoomGeometry.Clamp(room.OuterArea, next, halfView);
                next.x = center.x; next.y = center.y;
            }
            transform.position = next;
        }
    }
}
