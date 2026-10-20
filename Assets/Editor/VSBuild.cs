#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor.EditorTools
{
    /// Programmatic builder for the scene, prefabs and ScriptableObjects. Driven from the Unity
    /// CLI (`unity command eval`) so setup is reproducible instead of hand-wired in the Inspector.
    public static class VSBuild
    {
        const string ScenePath = "Assets/Scenes/Game.unity";

        static readonly Color RobotBody = Palette.Robot;
        static readonly Color NightBg = Palette.NightBg;

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

            var spawnerGO = new GameObject("EnemySpawner");
            spawnerGO.AddComponent<EnemySpawner>().Configure(
                AssetDatabase.LoadAssetAtPath<WaveData>("Assets/ScriptableObjects/WaveData_PhongKhach.asset"),
                new[]
                {
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/BuiBong.prefab"),
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/VunBanhMi.prefab"),
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CucTocRoi.prefab"),
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CucTocRoiNho.prefab"),
                },
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ConMeo.prefab"));

            var dustGO = new GameObject("DustSpawner");
            dustGO.AddComponent<DustSpawner>().Configure(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/Dust.prefab"));

            var dockGO = new GameObject("DockSpawner");
            dockGO.AddComponent<DockSpawner>().Configure(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/ChargingDock.prefab"));

            var canvas = BuildCanvas();

            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(canvas.transform, false);
            hudGO.AddComponent<HUD>();

            var levelUpGO = new GameObject("LevelUpPanel", typeof(RectTransform));
            levelUpGO.transform.SetParent(canvas.transform, false);
            var levelUpPanel = levelUpGO.AddComponent<LevelUpPanel>();

            var endGO = new GameObject("EndScreen", typeof(RectTransform));
            endGO.transform.SetParent(canvas.transform, false);
            endGO.AddComponent<EndScreen>();

            var umGO = new GameObject("UpgradeManager");
            umGO.AddComponent<UpgradeManager>().Configure(LoadUpgrades(), levelUpPanel);

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

            var ringGO = new GameObject("SuctionRing");
            ringGO.transform.SetParent(player.transform, false);
            var ringSr = ringGO.AddComponent<SpriteRenderer>();
            ringSr.sortingOrder = 1;
            ringSr.color = new Color(Palette.Accent.r, Palette.Accent.g, Palette.Accent.b, 0.12f);
            var ringPs = ringGO.AddComponent<ProceduralSprite>();
            ringPs.Color = Color.white;
            ringPs.Eyes = false;
            ringPs.Pixels = 64;

            var suction = player.AddComponent<SuctionWeapon>();
            suction.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/LucHut.asset");
            suction.Ring = ringGO.transform;
            player.AddComponent<WeaponManager>();
            player.AddComponent<PlayerMagnet>();

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

        [MenuItem("VacuumSurvivor/Build Content")]
        public static string BuildContent()
        {
            EnsureFolder("Assets/ScriptableObjects/Enemies");
            EnsureFolder("Assets/Prefabs/Enemies");

            var buiBong = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/BuiBong.asset", d =>
            {
                d.enemyName = "BuiBong";
                d.maxHealth = 10f; d.moveSpeed = 1.8f; d.contactDamage = 5f; d.dustValue = 1;
                d.bodyColor = Palette.DustBunny; d.bodyPixels = 44;
                d.splitsOnDeath = false;
            });
            CreateEnemyPrefab("Assets/Prefabs/Enemies/BuiBong.prefab", buiBong, typeof(ChaserEnemy), 0.35f);

            var banhMi = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/VunBanhMi.asset", d =>
            {
                d.enemyName = "VunBanhMi";
                d.maxHealth = 6f; d.moveSpeed = 3.5f; d.contactDamage = 3f; d.dustValue = 1;
                d.bodyColor = Palette.Breadcrumb; d.bodyPixels = 34;
                d.swarmMin = 5; d.swarmMax = 8;
                d.splitsOnDeath = false;
            });
            CreateEnemyPrefab("Assets/Prefabs/Enemies/VunBanhMi.prefab", banhMi, typeof(ChaserEnemy), 0.28f);

            var tocNho = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/CucTocRoiNho.asset", d =>
            {
                d.enemyName = "CucTocRoiNho";
                d.maxHealth = 10f; d.moveSpeed = 1.4f; d.contactDamage = 5f; d.dustValue = 1;
                d.bodyColor = Palette.HairClump; d.bodyPixels = 34;
                d.splitsOnDeath = false;
            });
            var tocNhoPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemies/CucTocRoiNho.prefab", tocNho, typeof(ChaserEnemy), 0.28f);

            var tocRoi = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/CucTocRoi.asset", d =>
            {
                d.enemyName = "CucTocRoi";
                d.maxHealth = 30f; d.moveSpeed = 1.2f; d.contactDamage = 8f; d.dustValue = 3;
                d.bodyColor = Palette.HairClump; d.bodyPixels = 58;
                d.splitsOnDeath = true; d.splitInto = tocNho; d.splitCount = 2;
            });
            var tocRoiPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemies/CucTocRoi.prefab", tocRoi, typeof(SplitterEnemy), 0.5f);
            tocRoiPrefab.GetComponent<SplitterEnemy>().ChildPrefab = tocNhoPrefab;
            EditorUtility.SetDirty(tocRoiPrefab);
            PrefabUtility.SavePrefabAsset(tocRoiPrefab);

            var conMeo = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/ConMeo.asset", d =>
            {
                d.enemyName = "Con mèo";
                d.maxHealth = 400f; d.moveSpeed = 2f; d.contactDamage = 10f; d.dustValue = 40;
                d.bodyColor = Palette.Cat; d.bodyPixels = 96;
                d.splitsOnDeath = false;
            });
            CreateBossPrefab("Assets/Prefabs/Enemies/ConMeo.prefab", conMeo, tocRoi);

            EnsureFolder("Assets/ScriptableObjects/Weapons");
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/LucHut.asset", w =>
            {
                w.weaponName = "LucHut"; w.maxLevel = 5;
                w.dps = new[] { 4f, 5.5f, 7f, 8.5f, 10f };
                w.radius = new[] { 1.5f, 1.8f, 2.1f, 2.45f, 2.8f };
            });

            CreateOrLoadAsset<WaveData>("Assets/ScriptableObjects/WaveData_PhongKhach.asset", w =>
            {
                w.entries = new[]
                {
                    new WaveEntry { startTime = 0f,   spawnInterval = 1.0f,  maxAlive = 25, types = new[] { buiBong } },
                    new WaveEntry { startTime = 30f,  spawnInterval = 0.85f, maxAlive = 40, types = new[] { buiBong, banhMi } },
                    new WaveEntry { startTime = 75f,  spawnInterval = 0.7f,  maxAlive = 55, types = new[] { buiBong, banhMi, tocRoi } },
                    new WaveEntry { startTime = 120f, spawnInterval = 0.6f,  maxAlive = 70, types = new[] { buiBong, banhMi, tocRoi } },
                };
            });

            EnsureFolder("Assets/Prefabs/Pickups");
            CreateDustPrefab("Assets/Prefabs/Pickups/Dust.prefab");
            CreateDockPrefab("Assets/Prefabs/Pickups/ChargingDock.prefab");

            EnsureFolder("Assets/ScriptableObjects/Upgrades");
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/LucHutCap.asset", u =>
            { u.type = UpgradeType.SuctionLevel; u.title = "Lực hút mạnh hơn"; u.description = "Tăng 1 cấp Lực hút: sát thương và bán kính lớn hơn."; u.valuePerLevel = new[] { 1f, 1f, 1f, 1f }; u.maxLevel = 4; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/PinLon.asset", u =>
            { u.type = UpgradeType.MaxBattery; u.title = "Pin dung lượng lớn"; u.description = "+20 pin tối đa, hồi luôn phần vừa tăng."; u.valuePerLevel = new[] { 20f, 20f, 20f, 20f, 20f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/DongCo.asset", u =>
            { u.type = UpgradeType.MoveSpeed; u.title = "Động cơ mạnh"; u.description = "+0,4 tốc độ di chuyển."; u.valuePerLevel = new[] { 0.4f, 0.4f, 0.4f, 0.4f, 0.4f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/NamCham.asset", u =>
            { u.type = UpgradeType.MagnetRadius; u.title = "Nam châm"; u.description = "+35% bán kính nhặt bụi."; u.valuePerLevel = new[] { 0.35f, 0.35f, 0.35f, 0.35f, 0.35f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/LocHEPA.asset", u =>
            { u.type = UpgradeType.SuctionDamage; u.title = "Lọc HEPA"; u.description = "+12% sát thương Lực hút."; u.valuePerLevel = new[] { 0.12f, 0.12f, 0.12f, 0.12f, 0.12f }; u.maxLevel = 5; });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return "content built";
        }

        static Canvas BuildCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            return canvas;
        }

        static List<UpgradeData> LoadUpgrades()
        {
            string[] names = { "LucHutCap", "PinLon", "DongCo", "NamCham", "LocHEPA" };
            var list = new List<UpgradeData>();
            foreach (var n in names)
            {
                var u = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/" + n + ".asset");
                if (u != null) list.Add(u);
            }
            return list;
        }

        static T CreateOrLoadAsset<T>(string path, System.Action<T> init) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(a, path);
            }
            init(a);
            EditorUtility.SetDirty(a);
            return a;
        }

        static EnemyData CreateOrLoadEnemyData(string path, System.Action<EnemyData> init)
        {
            var d = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(d, path);
            }
            init(d);
            EditorUtility.SetDirty(d);
            return d;
        }

        static GameObject CreateEnemyPrefab(string path, EnemyData data, System.Type behaviour, float colliderRadius)
        {
            var go = new GameObject(data.enemyName);
            go.layer = LayerMask.NameToLayer("Enemy");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Color.white;   // tinted per-type by EnemyBase; white base lets it flash
            ps.Pixels = data.bodyPixels;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = colliderRadius;
            ((EnemyBase)go.AddComponent(behaviour)).Data = data;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateBossPrefab(string path, EnemyData data, EnemyData hairball)
        {
            var go = new GameObject(data.enemyName);
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = Vector3.one * 2.5f; // ~8x the robot's footprint
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 6;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Color.white;   // tinted to bodyColor by EnemyBase; white base lets it flash
            ps.Cat = true;
            ps.Pixels = data.bodyPixels;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = 0.6f;

            var boss = go.AddComponent<BossCat>();
            boss.Data = data;
            boss.HairballData = hairball;
            boss.TelegraphColor = new Color(0.95f, 0.3f, 0.3f);
            var so = new SerializedObject(boss);
            so.FindProperty("knockback").floatValue = 0f; // the boss is too big to be shoved
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateDustPrefab(string path)
        {
            var go = new GameObject("Dust");
            go.layer = LayerMask.NameToLayer("Pickup");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Palette.DustMote;
            ps.Eyes = false;
            ps.Pixels = 16;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            go.AddComponent<Dust>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateDockPrefab(string path)
        {
            var go = new GameObject("ChargingDock");
            go.layer = LayerMask.NameToLayer("Pickup");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            sr.color = new Color(Palette.Accent.r, Palette.Accent.g, Palette.Accent.b, 0.5f);
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Color.white;
            ps.Eyes = false;
            ps.Pixels = 96;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.9f;
            go.AddComponent<ChargingDock>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
