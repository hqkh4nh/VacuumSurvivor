using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class BossArtTests
{
    const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    const string Art = "Assets/Art/Soft2D/Bosses/";

    [TestCase("ConMeo", 400f, 2f, 10f, 40)]
    [TestCase("ConMeo2", 600f, 2.5f, 10f, 50)]
    [TestCase("MayHutBui", 800f, 1.5f, 12f, 60)]
    public void Boss_prefabs_use_PNGs_with_the_original_size_collider_and_combat_stats(string name, float health, float speed, float damage, int dust)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        Assert.That(sprite, Is.Not.Null, name);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + name + ".prefab");
        Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
        Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
        Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one * 2.5f));
        Assert.That(prefab.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.6f));
        Assert.That(prefab.GetComponent<Rigidbody2D>().freezeRotation, Is.True);
        var data = prefab.GetComponent<EnemyBase>().Data;
        Assert.That(data.maxHealth, Is.EqualTo(health));
        Assert.That(data.moveSpeed, Is.EqualTo(speed));
        Assert.That(data.contactDamage, Is.EqualTo(damage));
        Assert.That(data.dustValue, Is.EqualTo(dust));
        Assert.That(data.bodyColor, Is.EqualTo(Color.white));
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name + ".png");
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
        Assert.That(importer.DoesSourceTextureHaveAlpha(), Is.True);
        Assert.That(importer.mipmapEnabled, Is.False);
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
    }

    [TestCase("ConMeo")]
    [TestCase("ConMeo2")]
    [TestCase("MayHutBui")]
    public void Painted_boss_keeps_red_attack_warning_hit_feedback_and_health_events(string name)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + name + ".prefab");
        var go = Object.Instantiate(prefab);
        float spawnedHealth = 0f;
        float reportedHealth = 0f;
        System.Action<float> onSpawn = value => spawnedHealth = value;
        System.Action<float, float> onHealth = (value, max) => reportedHealth = value;
        GameEvents.BossSpawned += onSpawn;
        GameEvents.BossHealthChanged += onHealth;
        try
        {
            var boss = go.GetComponent<EnemyBase>();
            typeof(EnemyBase).GetMethod("Awake", Private).Invoke(boss, null);
            boss.GetType().GetMethod("OnEnable", Private).Invoke(boss, null);
            Assert.That(spawnedHealth, Is.EqualTo(boss.Data.maxHealth));
            var renderer = go.GetComponent<SpriteRenderer>();
            var sprite = renderer.sprite;
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            var warning = boss.GetType().GetMethod("ApplyTelegraph", Private);
            warning.Invoke(boss, new object[] { true });
            Assert.That(renderer.color.r, Is.GreaterThan(renderer.color.g));
            Assert.That(renderer.color.r, Is.GreaterThan(renderer.color.b));
            warning.Invoke(boss, new object[] { false });
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            boss.TakeDamage(1f);
            Assert.That(renderer.color, Is.Not.EqualTo(Color.white));
            Assert.That(reportedHealth, Is.EqualTo(boss.Data.maxHealth - 1f));
            Assert.That(renderer.sprite, Is.SameAs(sprite));
            if (boss is BossCat cat) Assert.That(cat.HairballData.name, Is.EqualTo("CucTocRoi"));
            if (boss is BossVacuum vacuum) Assert.That(vacuum.BlowData.name, Is.EqualTo("BuiBong"));
        }
        finally
        {
            GameEvents.BossSpawned -= onSpawn;
            GameEvents.BossHealthChanged -= onHealth;
            Object.DestroyImmediate(go);
        }
    }

    [TestCase("ConMeo", "CreateBossPrefab")]
    [TestCase("ConMeo2", "CreateBossPrefab")]
    [TestCase("MayHutBui", "CreateVacuumBossPrefab")]
    public void Content_builder_recreates_bosses_with_the_imported_sprite(string name, string methodName)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        Assert.That(sprite, Is.Not.Null);
        string folder = "__BossArtTest_" + System.Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets", folder);
        try
        {
            var builder = System.Type.GetType("VacuumSurvivor.EditorTools.VSBuild, Assembly-CSharp-Editor");
            Assert.That(builder, Is.Not.Null);
            var method = builder.GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var data = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/ScriptableObjects/Enemies/" + name + ".asset");
            string drop = name == "MayHutBui" ? "BuiBong" : "CucTocRoi";
            var spawnedEnemy = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/ScriptableObjects/Enemies/" + drop + ".asset");
            var rebuilt = (GameObject)method.Invoke(null, new object[] { "Assets/" + folder + "/" + name + ".prefab", data, spawnedEnemy });
            Assert.That(rebuilt.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
            Assert.That(rebuilt.GetComponent<ProceduralSprite>(), Is.Null);
            Assert.That(rebuilt.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.6f));
        }
        finally { AssetDatabase.DeleteAsset("Assets/" + folder); }
    }

    [Test]
    public void Campaign_scene_still_uses_the_same_three_boss_prefabs_in_order()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            GameManager manager = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<GameManager>() != null) manager = root.GetComponent<GameManager>();
            Assert.That(manager, Is.Not.Null);
            var levels = new SerializedObject(manager).FindProperty("levels");
            string[] names = { "ConMeo", "ConMeo2", "MayHutBui" };
            Assert.That(levels.arraySize, Is.EqualTo(3));
            for (int i = 0; i < names.Length; i++)
                Assert.That(levels.GetArrayElementAtIndex(i).FindPropertyRelative("bossPrefab").objectReferenceValue,
                    Is.SameAs(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + names[i] + ".prefab")));
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }
}
