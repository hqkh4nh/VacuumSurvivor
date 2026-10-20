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
        const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

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
            BuildFloor();

            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NightBg;
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            camGO.AddComponent<CameraFollow>().Target = player.transform;
            camGO.AddComponent<AudioListener>();   // the generated scene has no listener otherwise

            new GameObject("Sfx").AddComponent<Sfx>();

            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>().Configure(new[]
            {
                new LevelConfig
                {
                    name = "Phòng khách",
                    waves = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/ScriptableObjects/WaveData_PhongKhach.asset"),
                    bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ConMeo.prefab"),
                    floorTint = Color.white,
                    duration = 180f,
                },
                new LevelConfig
                {
                    name = "Phòng ngủ",
                    waves = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/ScriptableObjects/WaveData_PhongNgu.asset"),
                    bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/ConMeo2.prefab"),
                    floorTint = new Color(0.82f, 0.88f, 1f),
                    duration = 180f,
                },
                new LevelConfig
                {
                    name = "Nhà bếp",
                    waves = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/ScriptableObjects/WaveData_NhaBep.asset"),
                    bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/MayHutBui.prefab"),
                    floorTint = new Color(1f, 0.92f, 0.82f),
                    duration = 180f,
                },
            });

            // Created before the UI so it subscribes to Won/Lost first and finalises the score the
            // end screen then reads.
            new GameObject("RunTracker").AddComponent<RunTracker>();

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

            var powerUpGO = new GameObject("PowerUpSpawner");
            powerUpGO.AddComponent<PowerUpSpawner>().Configure(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/PowerUp.prefab"));

            var canvas = BuildCanvas();

            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(canvas.transform, false);
            hudGO.AddComponent<HUD>();

            var levelUpGO = new GameObject("LevelUpPanel", typeof(RectTransform));
            levelUpGO.transform.SetParent(canvas.transform, false);
            var levelUpPanel = levelUpGO.AddComponent<LevelUpPanel>();

            var bannerGO = new GameObject("LevelBanner", typeof(RectTransform));
            bannerGO.transform.SetParent(canvas.transform, false);
            bannerGO.AddComponent<LevelBanner>();

            var endGO = new GameObject("EndScreen", typeof(RectTransform));
            endGO.transform.SetParent(canvas.transform, false);
            endGO.AddComponent<EndScreen>();

            var umGO = new GameObject("UpgradeManager");
            umGO.AddComponent<UpgradeManager>().Configure(LoadUpgrades(), levelUpPanel);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildOrder();
            AssetDatabase.SaveAssets();
            return "built " + ScenePath;
        }

        [MenuItem("VacuumSurvivor/Build Main Menu")]
        public static string BuildMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NightBg;
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);

            var canvas = BuildCanvas();
            var menuGO = new GameObject("MainMenu", typeof(RectTransform));
            menuGO.transform.SetParent(canvas.transform, false);
            menuGO.AddComponent<MainMenuScreen>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            EnsureBuildOrder();
            AssetDatabase.SaveAssets();
            return "built " + MainMenuScenePath;
        }

        static void BuildFloor()
        {
            var go = new GameObject("Floor");
            go.AddComponent<SpriteRenderer>().sortingOrder = -100;
            go.AddComponent<ProceduralFloor>();
        }

        static GameObject BuildPlayer()
        {
            var player = new GameObject("Player");
            player.layer = LayerMask.NameToLayer("Player");
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            var ps = player.AddComponent<ProceduralSprite>();
            ps.Kind = SpriteKind.Robot;
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
            ledPs.Kind = SpriteKind.Led;
            player.AddComponent<BatteryLED>();

            var ringGO = new GameObject("SuctionRing");
            ringGO.transform.SetParent(player.transform, false);
            var ringSr = ringGO.AddComponent<SpriteRenderer>();
            ringSr.sortingOrder = 1;
            ringSr.color = new Color(Palette.Accent.r, Palette.Accent.g, Palette.Accent.b, 0.12f);
            var ringPs = ringGO.AddComponent<ProceduralSprite>();
            ringPs.Color = Color.white;
            ringPs.Kind = SpriteKind.Ring;
            ringPs.Pixels = 64;

            var suction = player.AddComponent<SuctionWeapon>();
            suction.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/LucHut.asset");
            suction.Ring = ringGO.transform;
            suction.Owned = true;

            BuildOrbitWeapon(player);
            BuildProjectileWeapon(player);
            BuildTrailWeapon(player);
            BuildLaserWeapon(player);
            BuildPushWeapon(player);

            player.AddComponent<WeaponManager>();
            player.AddComponent<PlayerMagnet>();

            return player;
        }

        static void BuildOrbitWeapon(GameObject player)
        {
            var orbit = player.AddComponent<OrbitWeapon>();
            orbit.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/ChoiQuay.asset");

            var pivot = new GameObject("BroomPivot");
            pivot.transform.SetParent(player.transform, false);

            const int broomCount = 4; // the weapon's level-5 broom count; OrbitWeapon shows only its current level's share
            var brooms = new Transform[broomCount];
            for (int i = 0; i < broomCount; i++)
            {
                var b = new GameObject("Broom" + i);
                b.transform.SetParent(pivot.transform, false);
                b.transform.localScale = Vector3.one * 0.6f;
                var sr = b.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 9;
                var ps = b.AddComponent<ProceduralSprite>();
                ps.Kind = SpriteKind.Broom;
                ps.Color = Palette.Breadcrumb;
                ps.Pixels = 24;
                brooms[i] = b.transform;
            }
            orbit.Pivot = pivot.transform;
            orbit.Brooms = brooms;
        }

        static void BuildProjectileWeapon(GameObject player)
        {
            var proj = player.AddComponent<ProjectileWeapon>();
            proj.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/VoiXit.asset");

            var poolGO = new GameObject("DropletPool");
            poolGO.transform.SetParent(player.transform, false);
            var pool = poolGO.AddComponent<GameObjectPool>();
            pool.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Droplet.prefab");
            proj.Pool = pool;
        }

        static void BuildTrailWeapon(GameObject player)
        {
            var trail = player.AddComponent<TrailWeapon>();
            trail.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/VetLau.asset");

            var poolGO = new GameObject("TrailPool");
            poolGO.transform.SetParent(player.transform, false);
            var pool = poolGO.AddComponent<GameObjectPool>();
            pool.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/TrailPatch.prefab");
            trail.Pool = pool;
        }

        static void BuildLaserWeapon(GameObject player)
        {
            var laser = player.AddComponent<LaserWeapon>();
            laser.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/TiaLidar.asset");
            // The beams are line renderers the weapon builds for itself at runtime.
        }

        static void BuildPushWeapon(GameObject player)
        {
            var push = player.AddComponent<PushWeapon>();
            push.Data = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/CoiBaoDong.asset");

            var ringGO = new GameObject("HornRing");
            ringGO.transform.SetParent(player.transform, false);
            var sr = ringGO.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            sr.color = new Color(Palette.Accent.r, Palette.Accent.g, Palette.Accent.b, 0.22f);
            var ps = ringGO.AddComponent<ProceduralSprite>();
            ps.Color = Color.white;
            ps.Kind = SpriteKind.Ring;
            ps.Pixels = 64;
            ringGO.SetActive(false);
            push.Ring = ringGO.transform;
        }

        static void MakeStatic(GameObject prefab)
        {
            var rb = prefab.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic; // LEGO and puddles don't get shoved
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
        }

        static GameObject CreateTrailPatchPrefab(string path)
        {
            var go = new GameObject("TrailPatch");   // Default layer; damages via OverlapCircleAll, no collider needed
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = new Color(Palette.Moonlight.r, Palette.Moonlight.g, Palette.Moonlight.b, 0.45f);
            ps.Kind = SpriteKind.MopTrail;
            ps.Pixels = 32;
            go.AddComponent<TrailPatch>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
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

        // Keep the menu at build index 0 and the game at index 1, skipping either if it has not been
        // built yet, so the player boots into the menu.
        static void EnsureBuildOrder()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var path in new[] { MainMenuScenePath, ScenePath })
                if (System.IO.File.Exists(path))
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
            CreateEnemyPrefab("Assets/Prefabs/Enemies/BuiBong.prefab", buiBong, typeof(ChaserEnemy), 0.35f, SpriteKind.DustBunny);

            var banhMi = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/VunBanhMi.asset", d =>
            {
                d.enemyName = "VunBanhMi";
                d.maxHealth = 6f; d.moveSpeed = 3.5f; d.contactDamage = 3f; d.dustValue = 1;
                d.bodyColor = Palette.Breadcrumb; d.bodyPixels = 34;
                d.swarmMin = 5; d.swarmMax = 8;
                d.splitsOnDeath = false;
            });
            CreateEnemyPrefab("Assets/Prefabs/Enemies/VunBanhMi.prefab", banhMi, typeof(ChaserEnemy), 0.28f, SpriteKind.Breadcrumb);

            var tocNho = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/CucTocRoiNho.asset", d =>
            {
                d.enemyName = "CucTocRoiNho";
                d.maxHealth = 10f; d.moveSpeed = 1.4f; d.contactDamage = 5f; d.dustValue = 1;
                d.bodyColor = Palette.HairClump; d.bodyPixels = 34;
                d.splitsOnDeath = false;
            });
            var tocNhoPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemies/CucTocRoiNho.prefab", tocNho, typeof(ChaserEnemy), 0.28f, SpriteKind.HairClump);

            var tocRoi = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/CucTocRoi.asset", d =>
            {
                d.enemyName = "CucTocRoi";
                d.maxHealth = 30f; d.moveSpeed = 1.2f; d.contactDamage = 8f; d.dustValue = 3;
                d.bodyColor = Palette.HairClump; d.bodyPixels = 58;
                d.splitsOnDeath = true; d.splitInto = tocNho; d.splitCount = 2;
            });
            var tocRoiPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemies/CucTocRoi.prefab", tocRoi, typeof(SplitterEnemy), 0.5f, SpriteKind.HairClump);
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

            var conMeo2 = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/ConMeo2.asset", d =>
            {
                d.enemyName = "Con mèo";
                d.maxHealth = 600f; d.moveSpeed = 2.5f; d.contactDamage = 10f; d.dustValue = 50;
                d.bodyColor = Palette.Cat; d.bodyPixels = 96;
                d.splitsOnDeath = false;
            });
            CreateBossPrefab("Assets/Prefabs/Enemies/ConMeo2.prefab", conMeo2, tocRoi);

            var mayHutBui = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/MayHutBui.asset", d =>
            {
                d.enemyName = "Máy hút bụi cũ";
                d.maxHealth = 800f; d.moveSpeed = 1.5f; d.contactDamage = 12f; d.dustValue = 60;
                d.bodyColor = new Color(0.5f, 0.5f, 0.58f); d.bodyPixels = 96;
                d.splitsOnDeath = false;
            });
            CreateVacuumBossPrefab("Assets/Prefabs/Enemies/MayHutBui.prefab", mayHutBui, buiBong);

            var tatLe = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/TatLe.asset", d =>
            {
                d.enemyName = "Tất lẻ";
                d.maxHealth = 20f; d.moveSpeed = 0f; d.contactDamage = 12f; d.dustValue = 2;
                d.bodyColor = new Color(0.6f, 0.62f, 0.72f); d.bodyPixels = 40;
                d.aimTime = 1f; d.dashSpeed = 9f; d.dashDuration = 0.5f; d.recoverTime = 0.6f;
                d.splitsOnDeath = false;
            });
            CreateEnemyPrefab("Assets/Prefabs/Enemies/TatLe.prefab", tatLe, typeof(DasherEnemy), 0.3f, SpriteKind.Sock);

            var cucLego = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/CucLego.asset", d =>
            {
                d.enemyName = "Cục LEGO";
                d.maxHealth = 1f; d.moveSpeed = 0f; d.contactDamage = 20f; d.dustValue = 0;
                d.bodyColor = new Color(0.82f, 0.4f, 0.42f); d.bodyPixels = 40;
                d.indestructible = true; d.lifetime = 25f;
            });
            MakeStatic(CreateEnemyPrefab("Assets/Prefabs/Enemies/CucLego.prefab", cucLego, typeof(HazardEnemy), 0.34f, SpriteKind.Lego));

            var vungNuoc = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/VungNuoc.asset", d =>
            {
                d.enemyName = "Vũng nước";
                d.maxHealth = 12f; d.moveSpeed = 0f; d.contactDamage = 4f; d.dustValue = 0;
                d.bodyColor = new Color(0.45f, 0.6f, 0.8f, 0.75f); d.bodyPixels = 56;
                d.indestructible = false; d.lifetime = 0f;
            });
            MakeStatic(CreateEnemyPrefab("Assets/Prefabs/Enemies/VungNuoc.prefab", vungNuoc, typeof(HazardEnemy), 0.6f, SpriteKind.Puddle));

            var daySac = CreateOrLoadEnemyData("Assets/ScriptableObjects/Enemies/DaySac.asset", d =>
            {
                d.enemyName = "Dây sạc";
                d.maxHealth = 25f; d.moveSpeed = 2.5f; d.contactDamage = 6f; d.dustValue = 3;
                d.bodyColor = new Color(0.8f, 0.8f, 0.85f); d.bodyPixels = 48;
                d.rootDuration = 1f; d.weaveAmp = 0.6f; d.weaveFreq = 6f;
                d.splitsOnDeath = false;
            });
            CreateEnemyPrefab("Assets/Prefabs/Enemies/DaySac.prefab", daySac, typeof(SnakeEnemy), 0.3f, SpriteKind.Cord);

            EnsureFolder("Assets/ScriptableObjects/Weapons");
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/LucHut.asset", w =>
            {
                w.weaponName = "Lực hút"; w.maxLevel = 5;
                w.dps = new[] { 4f, 5.5f, 7f, 8.5f, 10f };
                w.radius = new[] { 1.5f, 1.8f, 2.1f, 2.45f, 2.8f };
            });
            // Orbit/projectile per-level stats live on their weapon components; these assets only
            // carry the display name and max level.
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/ChoiQuay.asset", w =>
            { w.weaponName = "Chổi"; w.maxLevel = 5; });
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/VoiXit.asset", w =>
            { w.weaponName = "Vòi"; w.maxLevel = 5; });
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/VetLau.asset", w =>
            { w.weaponName = "Vệt lau"; w.maxLevel = 5; });
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/TiaLidar.asset", w =>
            { w.weaponName = "LIDAR"; w.maxLevel = 5; });
            CreateOrLoadAsset<WeaponData>("Assets/ScriptableObjects/Weapons/CoiBaoDong.asset", w =>
            { w.weaponName = "Còi"; w.maxLevel = 5; });

            CreateOrLoadAsset<WaveData>("Assets/ScriptableObjects/WaveData_PhongKhach.asset", w =>
            {
                w.entries = new[]
                {
                    new WaveEntry { startTime = 0f,   spawnInterval = 1.4f,  maxAlive = 16, types = new[] { buiBong } },
                    new WaveEntry { startTime = 30f,  spawnInterval = 1.1f,  maxAlive = 28, types = new[] { buiBong, banhMi, cucLego } },
                    new WaveEntry { startTime = 75f,  spawnInterval = 0.75f, maxAlive = 45, types = new[] { buiBong, banhMi, tocRoi, cucLego } },
                    new WaveEntry { startTime = 120f, spawnInterval = 0.62f, maxAlive = 60, types = new[] { buiBong, banhMi, tocRoi, tatLe } },
                };
            });

            CreateOrLoadAsset<WaveData>("Assets/ScriptableObjects/WaveData_PhongNgu.asset", w =>
            {
                w.entries = new[]
                {
                    new WaveEntry { startTime = 0f,   spawnInterval = 1.2f,  maxAlive = 22, types = new[] { buiBong, tocRoi } },
                    new WaveEntry { startTime = 30f,  spawnInterval = 0.9f,  maxAlive = 38, types = new[] { buiBong, banhMi, tocRoi, tatLe } },
                    new WaveEntry { startTime = 75f,  spawnInterval = 0.68f, maxAlive = 55, types = new[] { buiBong, banhMi, tocRoi, tatLe, daySac } },
                    new WaveEntry { startTime = 120f, spawnInterval = 0.52f, maxAlive = 72, types = new[] { buiBong, banhMi, tocRoi, tatLe, daySac, cucLego } },
                };
            });

            CreateOrLoadAsset<WaveData>("Assets/ScriptableObjects/WaveData_NhaBep.asset", w =>
            {
                w.entries = new[]
                {
                    new WaveEntry { startTime = 0f,   spawnInterval = 1.0f,  maxAlive = 30,  types = new[] { buiBong, banhMi, tocRoi } },
                    new WaveEntry { startTime = 30f,  spawnInterval = 0.8f,  maxAlive = 48,  types = new[] { buiBong, banhMi, tocRoi, daySac } },
                    new WaveEntry { startTime = 75f,  spawnInterval = 0.6f,  maxAlive = 72,  types = new[] { buiBong, banhMi, tocRoi, daySac, vungNuoc } },
                    new WaveEntry { startTime = 120f, spawnInterval = 0.45f, maxAlive = 100, types = new[] { buiBong, banhMi, tocRoi, tatLe, daySac, vungNuoc } },
                };
            });

            EnsureFolder("Assets/Prefabs/Pickups");
            CreateDustPrefab("Assets/Prefabs/Pickups/Dust.prefab");
            CreateDockPrefab("Assets/Prefabs/Pickups/ChargingDock.prefab");
            CreatePowerUpPrefab("Assets/Prefabs/Pickups/PowerUp.prefab");

            EnsureFolder("Assets/Prefabs/Weapons");
            CreateProjectilePrefab("Assets/Prefabs/Weapons/Droplet.prefab");
            CreateTrailPatchPrefab("Assets/Prefabs/Weapons/TrailPatch.prefab");

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
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/ChoiQuay.asset", u =>
            { u.type = UpgradeType.OrbitWeapon; u.title = "Chổi quay"; u.description = "Các cây chổi xoay quanh robot, chạm vào rác thì gây sát thương. Chọn lại để lên cấp."; u.valuePerLevel = new[] { 0f, 0f, 0f, 0f, 0f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/VoiXit.asset", u =>
            { u.type = UpgradeType.ProjectileWeapon; u.title = "Vòi xịt nước"; u.description = "Tự bắn viên nước về kẻ địch gần nhất. Chọn lại để lên cấp."; u.valuePerLevel = new[] { 0f, 0f, 0f, 0f, 0f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/VetLau.asset", u =>
            { u.type = UpgradeType.TrailWeapon; u.title = "Vệt lau sàn"; u.description = "Để lại vệt nước phía sau, rác đi qua bị mất máu. Chọn lại để lên cấp."; u.valuePerLevel = new[] { 0f, 0f, 0f, 0f, 0f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/TiaLidar.asset", u =>
            { u.type = UpgradeType.LaserWeapon; u.title = "Tia quét LIDAR"; u.description = "Quét một tia laser xoay vòng, gây sát thương rác trên đường. Chọn lại để lên cấp."; u.valuePerLevel = new[] { 0f, 0f, 0f, 0f, 0f }; u.maxLevel = 5; });
            CreateOrLoadAsset<UpgradeData>("Assets/ScriptableObjects/Upgrades/CoiBaoDong.asset", u =>
            { u.type = UpgradeType.PushWeapon; u.title = "Còi báo động"; u.description = "Định kỳ đẩy lùi rác xung quanh và gây sát thương nhẹ. Chọn lại để lên cấp."; u.valuePerLevel = new[] { 0f, 0f, 0f, 0f, 0f }; u.maxLevel = 5; });

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
            string[] names = { "LucHutCap", "PinLon", "DongCo", "NamCham", "LocHEPA", "ChoiQuay", "VoiXit", "VetLau", "TiaLidar", "CoiBaoDong" };
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

        static GameObject CreateEnemyPrefab(string path, EnemyData data, System.Type behaviour, float colliderRadius, SpriteKind kind)
        {
            var go = new GameObject(data.enemyName);
            go.layer = LayerMask.NameToLayer("Enemy");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Kind = kind;
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
            ps.Kind = SpriteKind.Cat;
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

        static GameObject CreateVacuumBossPrefab(string path, EnemyData data, EnemyData blow)
        {
            var go = new GameObject(data.enemyName);
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = Vector3.one * 2.5f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 6;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Color.white;   // tinted to bodyColor by EnemyBase; white base lets it flash
            ps.Kind = SpriteKind.Vacuum;
            ps.Pixels = data.bodyPixels;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = 0.6f;

            var boss = go.AddComponent<BossVacuum>();
            boss.Data = data;
            boss.BlowData = blow;
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
            ps.Kind = SpriteKind.DustMote;
            ps.Pixels = 16;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            go.AddComponent<Dust>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateProjectilePrefab(string path)
        {
            var go = new GameObject("Droplet");   // Default layer: Pickup↔Enemy collisions are off, triggers must still fire
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Palette.Moonlight;
            ps.Kind = SpriteKind.Droplet;
            ps.Pixels = 20;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            go.AddComponent<Projectile>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreatePowerUpPrefab(string path)
        {
            var go = new GameObject("PowerUp");
            go.layer = LayerMask.NameToLayer("Pickup");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            var ps = go.AddComponent<ProceduralSprite>();
            ps.Color = Palette.Moonlight;
            ps.Kind = SpriteKind.Battery;   // PowerUp.Init swaps the silhouette per drop
            ps.Pixels = 32;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            go.AddComponent<PowerUp>();

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
            ps.Kind = SpriteKind.Dock;
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
