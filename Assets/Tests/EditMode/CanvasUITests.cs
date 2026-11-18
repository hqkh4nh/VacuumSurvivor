using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class CanvasUITests
{
    [TestCase("MainMenuScreen")]
    [TestCase("HUD")]
    [TestCase("LevelUpPanel")]
    [TestCase("LevelBanner")]
    [TestCase("EndScreen")]
    public void Authored_UI_prefabs_have_assigned_controller_references(string name)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/" + name + ".prefab");
        Assert.That(prefab, Is.Not.Null);
        var controller = prefab.GetComponent(name);
        Assert.That(controller, Is.Not.Null);
        var property = new SerializedObject(controller).GetIterator();
        while (property.NextVisible(true))
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                Assert.That(property.objectReferenceValue, Is.Not.Null, name + "." + property.propertyPath);
    }

    [TestCase("MainMenuScreen", 6)]
    [TestCase("LevelUpPanel", 3)]
    [TestCase("EndScreen", 2)]
    public void Authored_buttons_have_persistent_controller_actions(string name, int expectedCount)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/" + name + ".prefab");
        int count = 0;
        foreach (var component in prefab.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (component.GetType().FullName != "UnityEngine.UI.Button") continue;
            count++;
            var calls = new SerializedObject(component).FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            Assert.That(calls.arraySize, Is.EqualTo(1));
            var call = calls.GetArrayElementAtIndex(0);
            var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
            string method = call.FindPropertyRelative("m_MethodName").stringValue;
            Assert.That(target, Is.Not.Null);
            Assert.That(target.GetType().GetMethod(method), Is.Not.Null, name + "." + method);
        }
        Assert.That(count, Is.EqualTo(expectedCount));
    }

    [Test]
    public void Upgrade_choice_uses_existing_cards_hides_unavailable_cards_and_calls_back_once()
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI/LevelUpPanel.prefab");
        var option = ScriptableObject.CreateInstance<UpgradeData>();
        float originalScale = Time.timeScale;
        try
        {
            var panel = root.GetComponent<LevelUpPanel>();
            int childCount = root.GetComponentsInChildren<Transform>(true).Length;
            int picked = -1;
            int calls = 0;
            panel.Show(new[] { option }, index => { picked = index; calls++; });
            Assert.That(panel.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.Zero);

            panel.Choose(2);
            Assert.That(calls, Is.Zero);
            panel.Choose(0);
            panel.Choose(0);
            Assert.That(picked, Is.Zero);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(panel.IsOpen, Is.False);
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(childCount));
        }
        finally
        {
            Time.timeScale = originalScale;
            Object.DestroyImmediate(option);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void Game_scene_keeps_the_upgrade_manager_connected_to_the_authored_panel()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            UpgradeManager manager = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var candidate = root.GetComponentInChildren<UpgradeManager>(true);
                if (candidate != null) manager = candidate;
            }
            Assert.That(manager, Is.Not.Null);
            var panel = new SerializedObject(manager).FindProperty("panel").objectReferenceValue as LevelUpPanel;
            Assert.That(panel, Is.Not.Null);
            Assert.That(panel.gameObject.scene, Is.EqualTo(scene));
            Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(panel), Is.Not.Null);
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }
}
