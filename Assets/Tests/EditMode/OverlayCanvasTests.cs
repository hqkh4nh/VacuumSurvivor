using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VacuumSurvivor;

public class OverlayCanvasTests
{
    const string Prefab = "Assets/Prefabs/UI/HUD.prefab";
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    static Component Overlay(GameObject root)
    {
        var overlay = root.GetComponent("GameplayOverlay");
        Assert.That(overlay, Is.Not.Null, "HUD must contain its authored overlay controller.");
        return overlay;
    }

    static T Reference<T>(Component controller, string field) where T : Object
    {
        var property = new SerializedObject(controller).FindProperty(field);
        Assert.That(property, Is.Not.Null, field);
        Assert.That(property.objectReferenceValue, Is.Not.Null, field);
        return property.objectReferenceValue as T;
    }

    static object Call(Component component, string method, params object[] args) =>
        component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(component, args);

    [Test]
    public void HUD_contains_authored_pause_marker_and_boss_damage_trail()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var overlay = Overlay(root);
        Assert.That(Reference<GameObject>(overlay, "pause").activeSelf, Is.False);
        Assert.That(Reference<RectTransform>(overlay, "marker").gameObject.activeSelf, Is.False);
        foreach (string field in new[] { "pauseCard", "resume", "volume", "arrow", "dockIcon" })
            Reference<Object>(overlay, field);
        var hud = root.GetComponent<HUD>();
        var main = Reference<Image>(hud, "bossFill");
        var trail = Reference<Image>(hud, "bossDamageTrail");
        Assert.That(trail.transform.parent, Is.SameAs(main.transform.parent));
        Assert.That(trail.transform.GetSiblingIndex(), Is.LessThan(main.transform.GetSiblingIndex()));
    }

    [Test]
    public void HUD_initialization_uses_existing_UI_objects()
    {
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        float volume = AudioListener.volume;
        try
        {
            int before = root.GetComponentsInChildren<Transform>(true).Length;
            Call(root.GetComponent<HUD>(), "Awake");
            Call(root.GetComponent<HUD>(), "Start");
            var overlay = Overlay(root);
            Call(overlay, "Awake");
            Call(overlay, "LateUpdate");
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(before));
            Assert.That(root.GetComponents<Component>().Length, Is.GreaterThan(0));
        }
        finally { AudioListener.volume = volume; PrefabUtility.UnloadPrefabContents(root); }
    }

    [Test]
    public void Pause_buttons_and_volume_slider_have_persistent_controller_actions()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var overlay = Overlay(root);
        var pause = Reference<GameObject>(overlay, "pause");
        var buttons = pause.GetComponentsInChildren<Button>(true);
        Assert.That(buttons.Length, Is.EqualTo(2));
        foreach (var button in buttons)
        {
            Assert.That(button.onClick.GetPersistentEventCount(), Is.EqualTo(1));
            Assert.That(button.onClick.GetPersistentTarget(0), Is.SameAs(overlay));
            Assert.That(overlay.GetType().GetMethod(button.onClick.GetPersistentMethodName(0)), Is.Not.Null);
        }
        var slider = Reference<Slider>(overlay, "volume");
        Assert.That(slider.minValue, Is.Zero);
        Assert.That(slider.maxValue, Is.EqualTo(1f));
        Assert.That(slider.onValueChanged.GetPersistentEventCount(), Is.EqualTo(1));
        Assert.That(slider.onValueChanged.GetPersistentTarget(0), Is.SameAs(overlay));
        Assert.That(slider.onValueChanged.GetPersistentMethodName(0), Is.EqualTo("SetVolume"));
    }

    [Test]
    public void Authored_pause_and_volume_controls_preserve_their_behavior()
    {
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        var managerObject = new GameObject("Pause behavior test");
        var instanceField = typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        object originalManager = instanceField.GetValue(null);
        float timeScale = Time.timeScale, audio = AudioListener.volume;
        const string key = "VacuumSurvivor.MasterVolume";
        bool hadVolume = PlayerPrefs.HasKey(key);
        float savedVolume = PlayerPrefs.GetFloat(key);
        try
        {
            var manager = managerObject.AddComponent<GameManager>();
            instanceField.SetValue(null, manager);
            Time.timeScale = 1f;
            var overlay = Overlay(root);
            Call(overlay, "Awake");
            manager.TogglePause();
            Call(overlay, "LateUpdate");
            Assert.That(Reference<GameObject>(overlay, "pause").activeSelf, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Call(overlay, "Resume");
            Call(overlay, "LateUpdate");
            Assert.That(manager.Current, Is.EqualTo(GameManager.State.Playing));
            Assert.That(Reference<GameObject>(overlay, "pause").activeSelf, Is.False);
            Call(overlay, "SetVolume", 0.37f);
            Assert.That(AudioListener.volume, Is.EqualTo(0.37f).Within(0.001f));
            Assert.That(PlayerPrefs.GetFloat(key), Is.EqualTo(0.37f).Within(0.001f));
        }
        finally
        {
            instanceField.SetValue(null, originalManager);
            Time.timeScale = timeScale;
            AudioListener.volume = audio;
            if (hadVolume) PlayerPrefs.SetFloat(key, savedVolume); else PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Object.DestroyImmediate(managerObject);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void Authored_dock_marker_points_to_offscreen_dock_and_hides_onscreen()
    {
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        var cameraObject = new GameObject("Dock marker camera");
        var dock = new GameObject("Dock marker target");
        float volume = AudioListener.volume;
        try
        {
            ((RectTransform)root.transform).sizeDelta = new Vector2(1280, 720);
            var overlay = Overlay(root);
            Call(overlay, "Awake");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0, 0, -10);
            overlay.GetType().GetField("worldCamera", Private).SetValue(overlay, camera);
            var renderer = dock.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/ChargingDock.prefab").GetComponent<SpriteRenderer>().sprite;
            dock.transform.position = new Vector3(30, 0, 0);
            Assert.That(Call(overlay, "PlaceMarker", dock.transform), Is.EqualTo(true));
            Assert.That(Reference<RectTransform>(overlay, "marker").anchoredPosition.x, Is.InRange(0.01f, 545.01f));
            Assert.That(Reference<Image>(overlay, "dockIcon").sprite, Is.SameAs(renderer.sprite));
            dock.transform.position = Vector3.zero;
            Assert.That(Call(overlay, "PlaceMarker", dock.transform), Is.EqualTo(false));
        }
        finally
        {
            AudioListener.volume = volume;
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(dock);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void Authored_boss_damage_trail_shows_recent_damage_without_cloning_UI()
    {
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var hud = root.GetComponent<HUD>();
            int count = root.GetComponentsInChildren<Transform>(true).Length;
            Call(hud, "Awake");
            Call(hud, "OnBossSpawned", 100f);
            Call(hud, "OnBossHealth", 70f, 100f);
            Assert.That(Reference<Image>(hud, "bossFill").rectTransform.anchorMax.x, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(Reference<Image>(hud, "bossDamageTrail").rectTransform.anchorMax.x, Is.EqualTo(1f));
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(count));
            Call(hud, "OnBossHealth", 0f, 100f);
            Assert.That(Reference<GameObject>(hud, "bossBar").activeSelf, Is.False);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
