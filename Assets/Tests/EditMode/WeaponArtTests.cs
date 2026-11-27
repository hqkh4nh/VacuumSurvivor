using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class WeaponArtTests
{
    const string Art = "Assets/Art/Soft2D/Weapons/";
    const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

    [TestCase("Droplet", 14)]
    [TestCase("TrailPatch", 1)]
    public void Weapon_prefabs_use_imported_sprites(string name, int order)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/" + name + ".prefab");
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        Assert.That(sprite, Is.Not.Null);
        Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
        Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
        Assert.That(prefab.GetComponent<SpriteRenderer>().sortingOrder, Is.EqualTo(order));
        Assert.That(prefab.layer, Is.EqualTo(0));
        if (name == "Droplet")
        {
            Assert.That(prefab.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.15f));
            Assert.That(prefab.GetComponent<CircleCollider2D>().isTrigger, Is.True);
            Assert.That(prefab.GetComponent<Rigidbody2D>().bodyType, Is.EqualTo(RigidbodyType2D.Kinematic));
        }
        else Assert.That(prefab.GetComponent<Collider2D>(), Is.Null);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name + ".png");
        Assert.That(importer.DoesSourceTextureHaveAlpha(), Is.True);
        Assert.That(importer.mipmapEnabled, Is.False);
    }

    [Test]
    public void Game_scene_has_four_PNG_brooms_and_separate_readable_effect_colors()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            OrbitWeapon orbit = null;
            SuctionWeapon suction = null;
            PushWeapon push = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<OrbitWeapon>() != null) orbit = root.GetComponent<OrbitWeapon>();
                if (root.GetComponent<SuctionWeapon>() != null) suction = root.GetComponent<SuctionWeapon>();
                if (root.GetComponent<PushWeapon>() != null) push = root.GetComponent<PushWeapon>();
            }
            Assert.That(orbit.Brooms.Length, Is.EqualTo(4));
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Broom.png");
            Assert.That(sprite, Is.Not.Null);
            foreach (var broom in orbit.Brooms)
            {
                Assert.That(broom.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
                Assert.That(broom.GetComponent<ProceduralSprite>(), Is.Null);
                Assert.That(broom.localScale, Is.EqualTo(Vector3.one * 0.6f));
            }
            Color suctionColor = suction.Ring.GetComponent<SpriteRenderer>().color;
            Color pushColor = push.Ring.GetComponent<SpriteRenderer>().color;
            Assert.That(suctionColor.g, Is.GreaterThan(suctionColor.r));
            Assert.That(suctionColor.a, Is.InRange(0.1f, 0.85f));
            Assert.That(pushColor.r, Is.GreaterThan(pushColor.b));
            Assert.That(suction.Ring.GetComponent<ProceduralSprite>().Pixels, Is.GreaterThanOrEqualTo(128));
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    [Test]
    public void Generated_weapon_ring_has_clear_center_soft_edge_and_unit_diameter()
    {
        var sprite = SpriteFactory.Build(SpriteKind.Ring, Color.white, 128);
        var texture = (Texture2D)sprite.texture;
        try
        {
            Assert.That(texture.GetPixel(64, 64).a, Is.Zero);
            Assert.That(texture.GetPixel(0, 0).a, Is.Zero);
            bool visible = false, antialiased = false;
            for (int x = 64; x < 128; x++)
            {
                float alpha = texture.GetPixel(x, 64).a;
                visible |= alpha > 0.8f;
                antialiased |= alpha > 0f && alpha < 0.8f;
            }
            Assert.That(visible, Is.True);
            Assert.That(antialiased, Is.True);
            Assert.That(sprite.bounds.size.x, Is.EqualTo(1f).Within(0.001f));
        }
        finally { Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
    }

    [Test]
    public void Push_pulse_fades_without_changing_its_damage_radius()
    {
        var root = new GameObject("Push effect test");
        var ring = new GameObject("Ring");
        ring.transform.SetParent(root.transform);
        try
        {
            var renderer = ring.AddComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 0.8f, 0.5f, 0.4f);
            var push = root.AddComponent<PushWeapon>();
            push.Ring = ring.transform;
            push.Owned = true;
            typeof(PushWeapon).GetMethod("Awake", Private).Invoke(push, null);
            typeof(PushWeapon).GetField("cooldownLeft", Private).SetValue(push, 999f);
            typeof(PushWeapon).GetField("pulseRadius", Private).SetValue(push, 4f);
            float pulseTime = (float)typeof(PushWeapon).GetField("pulseTime", Private).GetValue(push);
            typeof(PushWeapon).GetField("pulseLeft", Private).SetValue(push, Time.deltaTime + pulseTime * 0.5f);
            typeof(PushWeapon).GetMethod("Update", Private).Invoke(push, null);
            Assert.That(renderer.color.a, Is.EqualTo(0.2f).Within(0.005f));
            Assert.That(ring.transform.localScale.x, Is.EqualTo(4f).Within(0.005f));
            Assert.That(typeof(PushWeapon).GetField("pulseRadius", Private).GetValue(push), Is.EqualTo(4f));
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void Laser_beams_have_rounded_caps_taper_and_cool_colors()
    {
        var root = new GameObject("Laser art test");
        Material material = null;
        try
        {
            var laser = root.AddComponent<LaserWeapon>();
            typeof(LaserWeapon).GetMethod("Awake", Private).Invoke(laser, null);
            var lines = root.GetComponentsInChildren<LineRenderer>();
            Assert.That(lines.Length, Is.EqualTo(3));
            material = lines[0].sharedMaterial;
            foreach (var line in lines)
            {
                Assert.That(line.numCapVertices, Is.GreaterThanOrEqualTo(4));
                Assert.That(line.endWidth, Is.LessThan(line.startWidth));
                Assert.That(line.startColor.g, Is.GreaterThan(line.startColor.r));
                Assert.That(line.enabled, Is.False);
            }
        }
        finally { Object.DestroyImmediate(root); if (material != null) Object.DestroyImmediate(material); }
    }

    [Test]
    public void Suction_visual_tracks_exact_range_including_dock_overdrive()
    {
        var root = new GameObject("Suction art test");
        var ring = new GameObject("Ring");
        ring.transform.SetParent(root.transform);
        try
        {
            var renderer = ring.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.5f, 0.9f, 0.85f, 0.2f);
            var suction = root.AddComponent<SuctionWeapon>();
            suction.Ring = ring.transform;
            suction.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/LucHut.asset");
            typeof(SuctionWeapon).GetMethod("Awake", Private).Invoke(suction, null);
            typeof(SuctionWeapon).GetMethod("Update", Private).Invoke(suction, null);
            float diameter = ring.transform.localScale.x;
            float alpha = renderer.color.a;
            Assert.That(diameter, Is.EqualTo(suction.Data.radius[0] * 2f));
            suction.Overdrive = 2f;
            typeof(SuctionWeapon).GetMethod("Update", Private).Invoke(suction, null);
            Assert.That(ring.transform.localScale.x, Is.EqualTo(diameter * 2f));
            Assert.That(renderer.color.a, Is.GreaterThan(alpha));
        }
        finally { Object.DestroyImmediate(root); }
    }

    [TestCase("Droplet", "CreateProjectilePrefab")]
    [TestCase("TrailPatch", "CreateTrailPatchPrefab")]
    public void Builder_keeps_weapon_PNGs(string name, string methodName)
    {
        string folder = "__WeaponArtTest_" + System.Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets", folder);
        try
        {
            var builder = System.Type.GetType("VacuumSurvivor.EditorTools.VSBuild, Assembly-CSharp-Editor");
            var prefab = (GameObject)builder.GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .Invoke(null, new object[] { "Assets/" + folder + "/" + name + ".prefab" });
            Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
            Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png")));
        }
        finally { AssetDatabase.DeleteAsset("Assets/" + folder); }
    }
}
