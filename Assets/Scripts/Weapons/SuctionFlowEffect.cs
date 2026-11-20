using UnityEngine;

namespace VacuumSurvivor
{
    /// Draws short inward spirals and dust streaks inside the weapon's range indicator.
    public class SuctionFlowEffect : MonoBehaviour
    {
        const int FlowCount = 5;
        const int DustCount = 10;
        const int Points = 18;
        LineRenderer[] flows;
        LineRenderer[] dust;
        Material material;

        void Build()
        {
            if (flows != null) return;
            material = new Material(Shader.Find("Sprites/Default"));
            flows = new LineRenderer[FlowCount];
            dust = new LineRenderer[DustCount];
            for (int i = 0; i < FlowCount; i++)
                flows[i] = EffectLines.Create(transform, "Flow" + i, material, Points, 2);
            for (int i = 0; i < DustCount; i++)
                dust[i] = EffectLines.Create(transform, "Dust" + i, material, 2, 3);
        }

        public void Render(float radius, float time, float strength)
        {
            Build();
            float brightness = Mathf.Clamp(strength, 1f, 1.5f);
            for (int i = 0; i < FlowCount; i++)
            {
                float progress = Mathf.Repeat(time * 0.65f + i * 0.21f, 1f);
                float tail = Mathf.Max(0f, progress - 0.32f);
                float fade = Mathf.Clamp01(progress * 8f) * Mathf.Clamp01((1f - progress) * 7f);
                var line = flows[i];
                line.widthMultiplier = radius * 0.035f;
                line.startColor = new Color(0.4f, 0.86f, 0.83f, 0.08f * fade * brightness);
                line.endColor = new Color(0.83f, 1f, 0.94f, 0.85f * fade * brightness);
                for (int point = 0; point < Points; point++)
                {
                    float u = Mathf.Lerp(tail, progress, point / (float)(Points - 1));
                    float angle = i * Mathf.PI * 2f / FlowCount + time * 0.3f + u * 2.4f;
                    line.SetPosition(point, EffectLines.Polar(angle, Mathf.Lerp(0.46f, 0.07f, u)));
                }
            }
            for (int i = 0; i < DustCount; i++)
            {
                float progress = Mathf.Repeat(time * 0.9f + i * 0.618f, 1f);
                float angle = i * 2.4f + time * 0.25f + progress * 1.5f;
                float distance = Mathf.Lerp(0.48f, 0.04f, progress);
                float fade = Mathf.Clamp01(progress * 10f) * Mathf.Clamp01((1f - progress) * 8f);
                var line = dust[i];
                line.widthMultiplier = radius * 0.026f;
                line.startColor = new Color(0.85f, 0.94f, 0.76f, 0.15f * fade);
                line.endColor = new Color(1f, 0.93f, 0.7f, Mathf.Min(1f, fade * brightness));
                line.SetPosition(0, EffectLines.Polar(angle - 0.035f, Mathf.Min(0.49f, distance + 0.024f)));
                line.SetPosition(1, EffectLines.Polar(angle, distance));
            }
        }

        void OnDestroy()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
        }
    }

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
