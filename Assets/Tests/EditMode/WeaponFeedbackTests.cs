using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class WeaponFeedbackTests
{
    [Test]
    public void Trail_drops_stay_on_the_floor_when_player_moves()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            TrailWeapon trail = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<TrailWeapon>() != null) trail = root.GetComponent<TrailWeapon>();
            Assert.That(trail, Is.Not.Null);
            var pool = trail.Pool;
            Assert.That(pool.transform.IsChildOf(trail.transform), Is.False, "Floor patches must not inherit player movement.");
            Vector3 dropPosition = new Vector3(1f, 2f, 0f);
            var patch = pool.Get(dropPosition);
            patch.GetComponent<TrailPatch>().Init(5f, 3f, 0.4f, pool);
            trail.transform.position += new Vector3(5f, 0f, 0f);
            Assert.That(patch.transform.position, Is.EqualTo(dropPosition));
            pool.Return(patch);
            Assert.That(pool.Get(dropPosition), Is.SameAs(patch));
            Assert.That(patch.transform.position, Is.EqualTo(dropPosition));
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    [Test]
    public void Weapon_rings_have_a_readable_stroke_at_the_authored_resolution()
    {
        var sprite = SpriteFactory.Build(SpriteKind.Ring, Color.white, 256);
        var texture = (Texture2D)sprite.texture;
        try
        {
            int stroke = 0;
            for (int x = 128; x < 256; x++) if (texture.GetPixel(x, 128).a > 0.5f) stroke++;
            Assert.That(stroke, Is.GreaterThanOrEqualTo(6));
            Assert.That(texture.GetPixel(128, 128).a, Is.Zero);
        }
        finally { Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
    }

    [Test]
    public void Scene_effects_have_sufficient_opacity_and_a_visible_horn_pulse_duration()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var suction = root.GetComponent<SuctionWeapon>();
                if (suction == null) continue;
                Assert.That(suction.Ring.GetComponent<SpriteRenderer>().color.a, Is.InRange(0.6f, 0.85f));
                var push = root.GetComponent<PushWeapon>();
                Assert.That(push.Ring.GetComponent<SpriteRenderer>().color.a, Is.InRange(0.85f, 1f));
                Assert.That(new SerializedObject(push).FindProperty("pulseTime").floatValue, Is.InRange(0.6f, 1f));
                return;
            }
            Assert.Fail("Player weapons are missing.");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }
}
