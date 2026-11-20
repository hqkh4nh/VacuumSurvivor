using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VacuumSurvivor;

public class PickupArtTests
{
    const string Art = "Assets/Art/Soft2D/Pickups/";
    const string Prefabs = "Assets/Prefabs/Pickups/";
    const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

    [TestCase("Dust", "Dust", 0.15f, 3)]
    [TestCase("ChargingDock", "ChargingDock", 0.9f, 2)]
    [TestCase("PowerUp", "Battery", 0.4f, 4)]
    public void Pickups_use_imported_art_and_keep_trigger_geometry(string prefabName, string image, float radius, int order)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + image + ".png");
        Assert.That(sprite, Is.Not.Null);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefabName + ".prefab");
        Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
        Assert.That(prefab.GetComponent<SpriteRenderer>().color, Is.EqualTo(Color.white));
        Assert.That(prefab.GetComponent<SpriteRenderer>().sortingOrder, Is.EqualTo(order));
        Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
        Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(prefab.layer, Is.EqualTo(LayerMask.NameToLayer("Pickup")));
        Assert.That(prefab.GetComponent<CircleCollider2D>().isTrigger, Is.True);
        Assert.That(prefab.GetComponent<CircleCollider2D>().radius, Is.EqualTo(radius));
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art + image + ".png");
        Assert.That(importer.DoesSourceTextureHaveAlpha(), Is.True);
        Assert.That(importer.mipmapEnabled, Is.False);
    }

    [Test]
    public void Reused_powerup_changes_image_for_every_kind_and_keeps_effect_parameters()
    {
        var holder = new GameObject("Pickup pool test");
        try
        {
            var pool = holder.AddComponent<GameObjectPool>();
            pool.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "PowerUp.prefab");
            GameObject previous = null;
            foreach (var kind in new[] { PowerUpKind.Battery, PowerUpKind.Magnet, PowerUpKind.Bomb, PowerUpKind.Battery })
            {
                var go = pool.Get(Vector3.zero);
                if (previous != null) Assert.That(go, Is.SameAs(previous));
                var power = go.GetComponent<PowerUp>();
                typeof(PowerUp).GetMethod("Awake", Private).Invoke(power, null);
                power.Init(kind, 30f, 5f, 60f, 12f);
                var expected = AssetDatabase.LoadAssetAtPath<Sprite>(Art + kind + ".png");
                Assert.That(expected, Is.Not.Null);
                Assert.That(go.GetComponent<SpriteRenderer>().sprite, Is.SameAs(expected));
                Assert.That((PowerUpKind)typeof(PowerUp).GetField("kind", Private).GetValue(power), Is.EqualTo(kind));
                foreach (var pair in new[] { ("batteryAmount", 30f), ("bombRadius", 5f), ("bombDamage", 60f), ("lifetime", 12f), ("age", 0f) })
                    Assert.That(typeof(PowerUp).GetField(pair.Item1, Private).GetValue(power), Is.EqualTo(pair.Item2));
                pool.Return(go);
                previous = go;
            }
        }
        finally { Object.DestroyImmediate(holder); }
    }

    [Test]
    public void Reused_dock_restores_authored_opacity_after_fading()
    {
        var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "ChargingDock.prefab"));
        try
        {
            var dock = go.GetComponent<ChargingDock>();
            typeof(ChargingDock).GetMethod("Awake", Private).Invoke(dock, null);
            typeof(ChargingDock).GetMethod("OnEnable", Private).Invoke(dock, null);
            var renderer = go.GetComponent<SpriteRenderer>();
            Color authored = renderer.color;
            renderer.color = new Color(authored.r, authored.g, authored.b, 0f);
            go.SetActive(false);
            go.SetActive(true);
            typeof(ChargingDock).GetMethod("OnEnable", Private).Invoke(dock, null);
            Assert.That(renderer.color, Is.EqualTo(authored));
            var serialized = new SerializedObject(dock);
            foreach (var pair in new[] { ("chargePerSecond", 15f), ("slowMultiplier", 0.3f), ("overdrive", 2f), ("lifetime", 20f), ("fadeTime", 3f) })
                Assert.That(serialized.FindProperty(pair.Item1).floatValue, Is.EqualTo(pair.Item2));
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void Painted_powerups_still_charge_collect_dust_and_damage_enemies()
    {
        var holder = new GameObject("Pickup effect test");
        var player = new GameObject("Player");
        var dustObject = new GameObject("Dust");
        GameObject enemyObject = null;
        try
        {
            var stats = player.AddComponent<PlayerStats>();
            typeof(PlayerStats).GetMethod("Awake", Private).Invoke(stats, null);
            stats.AddBattery(-50f);
            var power = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "PowerUp.prefab"), holder.transform).GetComponent<PowerUp>();
            typeof(PowerUp).GetMethod("Awake", Private).Invoke(power, null);
            power.Init(PowerUpKind.Battery, 30f, 5f, 60f, 12f);
            typeof(PowerUp).GetMethod("Apply", Private).Invoke(power, new object[] { stats });
            Assert.That(stats.Battery, Is.EqualTo(80f));
            var dust = dustObject.AddComponent<Dust>();
            dust.Init(3);
            power.Init(PowerUpKind.Magnet, 30f, 5f, 60f, 12f);
            typeof(PowerUp).GetMethod("Apply", Private).Invoke(power, new object[] { stats });
            Assert.That(stats.Dust, Is.EqualTo(3));
            Assert.That(dustObject.activeSelf, Is.False);
            enemyObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ConMeo.prefab"));
            enemyObject.transform.position = power.transform.position;
            var enemy = enemyObject.GetComponent<EnemyBase>();
            typeof(EnemyBase).GetMethod("Awake", Private).Invoke(enemy, null);
            typeof(BossCat).GetMethod("OnEnable", Private).Invoke(enemy, null);
            Physics2D.SyncTransforms();
            power.Init(PowerUpKind.Bomb, 30f, 5f, 60f, 12f);
            typeof(PowerUp).GetMethod("Apply", Private).Invoke(power, new object[] { stats });
            var health = (Health)typeof(EnemyBase).GetField("health", Private).GetValue(enemy);
            Assert.That(health.Current, Is.EqualTo(enemy.Data.maxHealth - 60f));
        }
        finally
        {
            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(dustObject);
            if (enemyObject != null) Object.DestroyImmediate(enemyObject);
        }
    }

    [TestCase("Dust", "CreateDustPrefab", "Dust")]
    [TestCase("PowerUp", "CreatePowerUpPrefab", "Battery")]
    [TestCase("ChargingDock", "CreateDockPrefab", "ChargingDock")]
    public void Builder_keeps_pickup_art_when_recreating_content(string name, string methodName, string image)
    {
        string folder = "__PickupArtTest_" + System.Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets", folder);
        try
        {
            var builder = System.Type.GetType("VacuumSurvivor.EditorTools.VSBuild, Assembly-CSharp-Editor");
            var method = builder.GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var prefab = (GameObject)method.Invoke(null, new object[] { "Assets/" + folder + "/" + name + ".prefab" });
            Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + image + ".png");
            Assert.That(sprite, Is.Not.Null);
            Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
            if (name == "PowerUp")
            {
                var serialized = new SerializedObject(prefab.GetComponent<PowerUp>());
                foreach (string kind in new[] { "battery", "magnet", "bomb" })
                    Assert.That(serialized.FindProperty(kind + "Sprite").objectReferenceValue,
                        Is.SameAs(AssetDatabase.LoadAssetAtPath<Sprite>(Art + char.ToUpperInvariant(kind[0]) + kind.Substring(1) + ".png")));
            }
        }
        finally { AssetDatabase.DeleteAsset("Assets/" + folder); }
    }
}
