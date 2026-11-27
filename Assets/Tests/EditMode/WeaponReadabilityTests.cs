using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VacuumSurvivor;

public class WeaponReadabilityTests
{
    [TestCase("WeaponStroke", 1f)]
    [TestCase("WeaponSprite", 0f)]
    public void Contrast_materials_are_available_in_player_builds_and_compile(string name, float stroke)
    {
        var material = Resources.Load<Material>("Materials/" + name);
        Assert.That(material, Is.Not.Null);
        Assert.That(material.shader.isSupported, Is.True);
        Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
        Assert.That(material.GetFloat("_Stroke"), Is.EqualTo(stroke));
    }

    [Test]
    public void Reused_projectile_clears_its_previous_motion_streak()
    {
        var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Droplet.prefab"));
        try
        {
            var trail = go.GetComponent<TrailRenderer>();
            Assert.That(trail, Is.Not.Null);
            typeof(Projectile).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(go.GetComponent<Projectile>(), null);
            trail.AddPositions(new[] { Vector3.zero, Vector3.right });
            go.GetComponent<Projectile>().Launch(Vector2.up, 9f, 12f, null);
            Assert.That(trail.positionCount, Is.LessThanOrEqualTo(1));
            var points = new Vector3[trail.positionCount]; trail.GetPositions(points);
            foreach (var point in points) Assert.That(Vector3.Distance(point, go.transform.position), Is.LessThan(0.001f));
            Assert.That(go.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.15f));
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void Reused_wet_patch_restores_opacity_and_keeps_its_damage_footprint()
    {
        var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/TrailPatch.prefab"));
        try
        {
            var patch = go.GetComponent<TrailPatch>();
            typeof(TrailPatch).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(patch, null);
            var surface = go.GetComponent<SpriteRenderer>(); var color = surface.color;
            surface.color = Color.clear;
            patch.Init(5f, 3f, 0.4f, null);
            Assert.That(surface.color, Is.EqualTo(color));
            Assert.That(go.transform.localScale, Is.EqualTo(Vector3.one * 0.8f));
            Assert.That(go.GetComponent<Collider2D>(), Is.Null);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
