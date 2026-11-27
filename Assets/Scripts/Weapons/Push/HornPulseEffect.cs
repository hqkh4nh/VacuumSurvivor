using UnityEngine;

namespace VacuumSurvivor
{
    /// Draws staggered broken sound waves and a brief flash at the robot when the horn fires.
    public class HornPulseEffect : MonoBehaviour
    {
        const int WaveCount = 3;
        const int Arcs = 4;
        const int Points = 18;
        LineRenderer[] waves;
        LineRenderer[] flash;
        Material material;

        void Build()
        {
            if (waves != null) return;
            material = new Material(Resources.Load<Material>("Materials/WeaponStroke"));
            waves = new LineRenderer[WaveCount * Arcs];
            flash = new LineRenderer[8];
            for (int wave = 0; wave < WaveCount; wave++)
            for (int arc = 0; arc < Arcs; arc++)
                waves[wave * Arcs + arc] = EffectLines.Create(transform, "Wave" + wave + "Arc" + arc, material, Points, 16);
            for (int i = 0; i < flash.Length; i++)
                flash[i] = EffectLines.Create(transform, "Flash" + i, material, 2, 17);
        }

        public void Render(float radius, float progress)
        {
            Build();
            progress = Mathf.Clamp01(progress);
            // The parent already expands with the leading wave; compensate for its current scale.
            float diameter = Mathf.Max(0.001f, 2f * radius * progress);
            for (int wave = 0; wave < WaveCount; wave++)
            {
                float delay = wave * 0.12f;
                float u = (progress - delay) / (1f - delay);
                for (int arc = 0; arc < Arcs; arc++)
                {
                    var line = waves[wave * Arcs + arc];
                    line.enabled = u > 0f && u < 1f;
                    if (!line.enabled) continue;
                    float alpha = Mathf.Pow(1f - u, 0.7f);
                    line.widthMultiplier = radius * (0.05f - u * 0.018f);
                    line.startColor = new Color(1f, 0.43f, 0.12f, alpha * 0.85f);
                    line.endColor = new Color(1f, 0.85f, 0.34f, alpha);
                    float center = arc * Mathf.PI * 0.5f + wave * 0.16f;
                    center += Mathf.Sin(progress * 30f + arc) * 0.025f * (1f - progress);
                    for (int point = 0; point < Points; point++)
                    {
                        float angle = center + Mathf.Lerp(-0.59f, 0.59f, point / (float)(Points - 1));
                        line.SetPosition(point, EffectLines.Polar(angle, radius * u / diameter));
                    }
                }
            }
            for (int i = 0; i < flash.Length; i++)
            {
                var line = flash[i];
                line.enabled = progress > 0f && progress < 0.28f;
                if (!line.enabled) continue;
                float alpha = 1f - progress / 0.28f;
                float angle = i * Mathf.PI * 0.25f;
                float distance = 0.42f + progress * 0.3f;
                line.widthMultiplier = 0.09f;
                line.startColor = new Color(1f, 0.98f, 0.78f, alpha);
                line.endColor = new Color(1f, 0.77f, 0.38f, alpha);
                line.SetPosition(0, EffectLines.Polar(angle, distance / diameter));
                line.SetPosition(1, EffectLines.Polar(angle, (distance + 0.2f) / diameter));
            }
        }

        void OnDestroy()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
        }
    }
}
