using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class EnemyArtTests
{
    const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

    [TestCase("VunBanhMi", 0.28f, 6f, 3.5f, 3f)]
    [TestCase("CucTocRoi", 0.5f, 30f, 1.2f, 8f)]
    [TestCase("CucTocRoiNho", 0.28f, 10f, 1.4f, 5f)]
    [TestCase("TatLe", 0.3f, 20f, 0f, 12f)]
    [TestCase("CucLego", 0.34f, 1f, 0f, 20f)]
    [TestCase("VungNuoc", 0.6f, 12f, 0f, 4f)]
    [TestCase("DaySac", 0.3f, 25f, 2.5f, 6f)]
    public void Ordinary_prefabs_use_transparent_PNG_art_with_original_colliders_and_stats(string name, float radius, float health, float speed, float damage)
    {
        string imagePath = "Assets/Art/Soft2D/Enemies/" + name + ".png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
        Assert.That(sprite, Is.Not.Null, name);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + name + ".prefab");
        Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
        Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
        Assert.That(prefab.GetComponent<CircleCollider2D>().radius, Is.EqualTo(radius));
        var data = prefab.GetComponent<EnemyBase>().Data;
        Assert.That(data.maxHealth, Is.EqualTo(health));
        Assert.That(data.moveSpeed, Is.EqualTo(speed));
        Assert.That(data.contactDamage, Is.EqualTo(damage));
        Assert.That(data.bodyColor, Is.EqualTo(Color.white));
        var importer = (TextureImporter)AssetImporter.GetAtPath(imagePath);
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
        Assert.That(importer.DoesSourceTextureHaveAlpha(), Is.True);
        Assert.That(importer.mipmapEnabled, Is.False);
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
    }

    [Test]
    public void Every_wave_enemy_and_the_small_splitter_child_has_a_registered_spawn_pool()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            EnemySpawner spawner = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<EnemySpawner>() != null) spawner = root.GetComponent<EnemySpawner>();
            Assert.That(spawner, Is.Not.Null);
            var list = new SerializedObject(spawner).FindProperty("enemyPrefabs");
            var registered = new HashSet<EnemyData>();
            for (int i = 0; i < list.arraySize; i++)
                registered.Add(((GameObject)list.GetArrayElementAtIndex(i).objectReferenceValue).GetComponent<EnemyBase>().Data);
            foreach (string room in new[] { "PhongKhach", "PhongNgu", "NhaBep" })
            {
                var waves = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/ScriptableObjects/WaveData_" + room + ".asset");
                foreach (var wave in waves.entries)
                    foreach (var type in wave.types) Assert.That(registered.Contains(type), Is.True, "No pool for " + type.name);
            }
            var small = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/ScriptableObjects/Enemies/CucTocRoiNho.asset");
            Assert.That(registered.Contains(small), Is.True);
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    [Test]
    public void Returning_a_hazard_without_a_kill_releases_spawn_capacity()
    {
        var holder = new GameObject("Hazard spawner test");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CucLego.prefab");
        var spawner = holder.AddComponent<EnemySpawner>();
        int kills = 0;
        System.Action<Vector2, int> recordKill = (position, amount) => kills++;
        try
        {
            spawner.Configure(null, new[] { prefab });
            typeof(EnemySpawner).GetMethod("Awake", Private).Invoke(spawner, null);
            typeof(EnemySpawner).GetMethod("OnEnable", Private).Invoke(spawner, null);
            EnemyBase.EnemyDied += recordKill;
            var enemy = spawner.SpawnAt(prefab.GetComponent<EnemyBase>().Data, Vector2.zero).GetComponent<EnemyBase>();
            Assert.That(spawner.ActiveCount, Is.EqualTo(1));
            typeof(EnemyBase).GetMethod("ReturnToPool", Private).Invoke(enemy, null);
            Assert.That(spawner.ActiveCount, Is.Zero);
            Assert.That(kills, Is.Zero);
        }
        finally
        {
            EnemyBase.EnemyDied -= recordKill;
            typeof(EnemySpawner).GetMethod("OnDisable", Private).Invoke(spawner, null);
            Object.DestroyImmediate(holder);
        }
    }

    [Test]
    public void Splitter_still_produces_two_smaller_enemies_from_the_same_pool_registry()
    {
        var holder = new GameObject("Splitter spawner test");
        var big = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CucTocRoi.prefab");
        var small = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CucTocRoiNho.prefab");
        var spawner = holder.AddComponent<EnemySpawner>();
        try
        {
            spawner.Configure(null, new[] { big, small });
            typeof(EnemySpawner).GetMethod("Awake", Private).Invoke(spawner, null);
            typeof(EnemySpawner).GetMethod("OnEnable", Private).Invoke(spawner, null);
            var enemy = spawner.SpawnAt(big.GetComponent<EnemyBase>().Data, Vector2.zero).GetComponent<EnemyBase>();
            typeof(EnemyBase).GetMethod("Awake", Private).Invoke(enemy, null);
            typeof(EnemyBase).GetMethod("OnEnable", Private).Invoke(enemy, null);
            enemy.TakeDamage(100f);
            Assert.That(spawner.ActiveCount, Is.EqualTo(2));
            int children = 0;
            foreach (var live in holder.GetComponentsInChildren<EnemyBase>())
                if (live.Data == small.GetComponent<EnemyBase>().Data) children++;
            Assert.That(children, Is.EqualTo(2));
        }
        finally
        {
            typeof(EnemySpawner).GetMethod("OnDisable", Private).Invoke(spawner, null);
            Object.DestroyImmediate(holder);
        }
    }

    [Test]
    public void Painted_sock_keeps_its_dash_warning_and_resets_to_normal_color_when_reused()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/TatLe.prefab");
        var go = Object.Instantiate(prefab);
        try
        {
            var enemy = go.GetComponent<DasherEnemy>();
            typeof(EnemyBase).GetMethod("Awake", Private).Invoke(enemy, null);
            typeof(DasherEnemy).GetMethod("OnEnable", Private).Invoke(enemy, null);
            var renderer = go.GetComponent<SpriteRenderer>();
            var sprite = renderer.sprite;
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            typeof(DasherEnemy).GetMethod("Move", Private).Invoke(enemy, null);
            Assert.That(renderer.color.r, Is.GreaterThan(renderer.color.b));
            Assert.That(renderer.color, Is.Not.EqualTo(Color.white));
            Assert.That(renderer.sprite, Is.SameAs(sprite));
            typeof(DasherEnemy).GetMethod("OnEnable", Private).Invoke(enemy, null);
            Assert.That(renderer.color, Is.EqualTo(Color.white));
        }
        finally { Object.DestroyImmediate(go); }
    }
}
