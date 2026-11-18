using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class PngArtTests
{
    const string Art = "Assets/Art/Soft2D/";

    [TestCase("Robot.png", true)]
    [TestCase("DustBunny.png", true)]
    [TestCase("ParquetFloor.png", false)]
    public void Starter_art_imports_as_bilinear_sprites_without_mipmaps(string name, bool transparent)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name);
        Assert.That(sprite, Is.Not.Null, name);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name);
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.mipmapEnabled, Is.False);
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
        Assert.That(importer.DoesSourceTextureHaveAlpha(), Is.EqualTo(transparent));
    }

    [Test]
    public void Dust_bunny_prefab_uses_painted_art_without_a_procedural_override_or_color_tint()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/BuiBong.prefab");
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "DustBunny.png");
        Assert.That(sprite, Is.Not.Null);
        Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
        Assert.That(prefab.GetComponent<ProceduralSprite>(), Is.Null);
        Assert.That(prefab.GetComponent<EnemyBase>().Data.bodyColor, Is.EqualTo(Color.white));
        Assert.That(prefab.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.35f));
    }

    [Test]
    public void Game_scene_uses_robot_and_tiled_floor_PNGs_with_the_original_colliders()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            PlayerController player = null;
            ProceduralFloor floor = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<PlayerController>() != null) player = root.GetComponent<PlayerController>();
                if (root.GetComponent<ProceduralFloor>() != null) floor = root.GetComponent<ProceduralFloor>();
            }
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.SameAs(AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Robot.png")));
            Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(player.GetComponent<ProceduralSprite>(), Is.Null);
            Assert.That(player.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.45f));
            Assert.That(player.GetComponent<BatteryLED>(), Is.Not.Null);
            Assert.That(floor, Is.Not.Null);
            var renderer = floor.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sprite, Is.SameAs(AssetDatabase.LoadAssetAtPath<Sprite>(Art + "ParquetFloor.png")));
            Assert.That(renderer.sprite, Is.Not.Null);
            Assert.That(renderer.drawMode, Is.EqualTo(SpriteDrawMode.Tiled));
            Assert.That(renderer.size, Is.EqualTo(new Vector2(32f, 24f)));
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    [Test]
    public void Floor_startup_preserves_an_authored_sprite()
    {
        var go = new GameObject("Floor test");
        var texture = new Texture2D(4, 4);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
        try
        {
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            var floor = go.AddComponent<ProceduralFloor>();
            typeof(ProceduralFloor).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(floor, null);
            Assert.That(go.GetComponent<SpriteRenderer>().sprite, Is.SameAs(sprite));
        }
        finally
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void Painted_enemy_flashes_on_damage_and_restores_its_color_when_reused()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/BuiBong.prefab");
        var go = Object.Instantiate(prefab);
        try
        {
            var enemy = go.GetComponent<EnemyBase>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(EnemyBase).GetMethod("Awake", flags).Invoke(enemy, null);
            typeof(EnemyBase).GetMethod("OnEnable", flags).Invoke(enemy, null);
            var renderer = go.GetComponent<SpriteRenderer>();
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            enemy.TakeDamage(1f);
            Assert.That(renderer.color, Is.Not.EqualTo(Color.white));
            typeof(EnemyBase).GetMethod("OnEnable", flags).Invoke(enemy, null);
            Assert.That(renderer.color, Is.EqualTo(Color.white));
        }
        finally { Object.DestroyImmediate(go); }
    }
}
