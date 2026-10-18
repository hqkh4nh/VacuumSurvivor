using UnityEngine;

namespace VacuumSurvivor
{
    /// Smoothly tracks a target in the XY plane, keeping the camera's own depth.
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smooth = 8f;

        public Transform Target { get => target; set => target = value; }

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 goal = target.position;
            goal.z = transform.position.z;
            // Exponential smoothing: frame-rate independent, no overshoot.
            transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }
    }
}
