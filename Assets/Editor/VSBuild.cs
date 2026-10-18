#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VacuumSurvivor.EditorTools
{
    /// Programmatic builder for the scene, prefabs and ScriptableObjects. Driven from the Unity
    /// CLI (`unity command eval`) so setup is reproducible instead of hand-wired in the Inspector.
    public static class VSBuild
    {
        const string ScenePath = "Assets/Scenes/Game.unity";

        static readonly Color RobotBody = new Color(0.91f, 0.91f, 0.94f); // #E8E8F0
        static readonly Color NightBg = new Color(0.106f, 0.106f, 0.184f); // #1B1B2F

        [MenuItem("VacuumSurvivor/Build Game Scene")]
        public static string BuildGameScene()
        {
            EnsureLayer("Player");
            EnsureLayer("Enemy");
            EnsureLayer("Pickup");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var player = BuildPlayer();

            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NightBg;
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            camGO.AddComponent<CameraFollow>().Target = player.transform;

            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            AssetDatabase.SaveAssets();
            return "built " + ScenePath;
        }

        static GameObject BuildPlayer()
        {
            var player = new GameObject("Player");
            player.layer = LayerMask.NameToLayer("Player");
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            var ps = player.AddComponent<ProceduralSprite>();
            ps.Color = RobotBody;
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            player.AddComponent<PlayerStats>();
            player.AddComponent<PlayerController>();

            var ledGO = new GameObject("LED");
            ledGO.transform.SetParent(player.transform, false);
            ledGO.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            ledGO.transform.localScale = Vector3.one * 0.3f;
            var ledSr = ledGO.AddComponent<SpriteRenderer>();
            ledSr.sortingOrder = 11;
            var ledPs = ledGO.AddComponent<ProceduralSprite>();
            ledPs.Color = Color.white;
            ledPs.Eyes = false;
            player.AddComponent<BatteryLED>();

            return player;
        }

        static void EnsureLayer(string layerName)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return;
            for (int i = 8; i < layers.arraySize; i++)
            {
                var el = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue))
                {
                    el.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
        }

        static void AddSceneToBuild(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
