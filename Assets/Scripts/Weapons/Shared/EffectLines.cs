using UnityEngine;

namespace VacuumSurvivor
{
    internal static class EffectLines
    {
        static readonly AnimationCurve Taper = new AnimationCurve(new Keyframe(0f, 0.15f), new Keyframe(0.65f, 1f), new Keyframe(1f, 0.3f));

        internal static LineRenderer Create(Transform parent, string name, Material material, int points, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.positionCount = points;
            line.numCapVertices = 6;
            line.numCornerVertices = 4;
            line.sortingOrder = order;
            line.widthCurve = Taper;
            return line;
        }

        internal static Vector3 Polar(float angle, float radius) => new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
    }
}
