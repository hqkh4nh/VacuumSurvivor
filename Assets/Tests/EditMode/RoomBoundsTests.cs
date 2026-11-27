using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VacuumSurvivor;

public class RoomBoundsTests
{
    static readonly Rect Room = new Rect(-16f, -12f, 32f, 24f);

    static Vector2 Call(string method, params object[] arguments)
    {
        var type = typeof(Campaign).Assembly.GetType("VacuumSurvivor.RoomGeometry");
        Assert.That(type, Is.Not.Null, "Shared room geometry is missing");
        return (Vector2)type.GetMethod(method).Invoke(null, arguments);
    }

    [TestCase(-100f, -100f, -15.55f, -11.55f)]
    [TestCase(100f, 100f, 15.55f, 11.55f)]
    [TestCase(3f, 4f, 3f, 4f)]
    public void Clamp_keeps_the_whole_robot_inside_the_room(float x, float y, float expectedX, float expectedY)
    {
        var actual = Call("Clamp", Room, new Vector2(x, y), Vector2.one * 0.45f);
        Assert.That(Vector2.Distance(actual, new Vector2(expectedX, expectedY)), Is.LessThan(0.001f));
    }

    [Test]
    public void Camera_clamps_by_its_full_viewport_and_centers_an_oversized_view()
    {
        var outer = new Rect(-16.5f, -12.5f, 33f, 25f);
        var actual = Call("Clamp", outer, new Vector2(100f, 100f), new Vector2(6f * 16f / 9f, 6f));
        Assert.That(Vector2.Distance(actual, new Vector2(5.833333f, 6.5f)), Is.LessThan(0.001f));
        Assert.That(Call("Clamp", outer, new Vector2(100f, 100f), Vector2.one * 40f), Is.EqualTo(Vector2.zero));
    }

    [Test]
    public void Spawn_positions_stay_inside_and_offscreen_even_when_player_is_in_a_corner()
    {
        var view = new Rect(-4.833333f, 0.5f, 21.333333f, 12f);
        var expanded = Rect.MinMaxRect(view.xMin - 1.5f, view.yMin - 1.5f, view.xMax + 1.5f, view.yMax + 1.5f);
        for (int degrees = 0; degrees < 360; degrees += 5)
        {
            var spawn = Call("Spawn", Room, new Vector2(15.55f, 11.55f), view, 1.5f, 8f, (float)degrees);
            Assert.That(spawn.x, Is.InRange(-14.5f, 14.5f));
            Assert.That(spawn.y, Is.InRange(-10.5f, 10.5f));
            Assert.That(expanded.Contains(spawn), Is.False);
            Assert.That(Vector2.Distance(spawn, new Vector2(15.55f, 11.55f)), Is.GreaterThan(3f));
        }
    }

    [Test]
    public void Spawn_falls_back_to_a_distant_safe_point_when_the_entire_room_is_visible()
    {
        var spawn = Call("Spawn", Room, new Vector2(15f, 11f), new Rect(-100f, -100f, 200f, 200f), 0.6f, 8f, 30f);
        Assert.That(spawn.x, Is.InRange(-15.4f, 15.4f));
        Assert.That(spawn.y, Is.InRange(-11.4f, 11.4f));
        Assert.That(Vector2.Distance(spawn, new Vector2(15f, 11f)), Is.GreaterThan(8f));
    }

    [Test]
    public void Room_prefab_has_a_small_floor_and_four_solid_walls()
    {
        var room = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Maps/Room.prefab");
        Assert.That(room, Is.Not.Null);
        Assert.That(room.GetComponent("RoomBounds"), Is.Not.Null);
        Assert.That(room.GetComponent<RoomBounds>().Size, Is.EqualTo(new Vector2(32f, 24f)));
        var layout = room.GetComponentInChildren<RoomLayout>();
        var walls = new SerializedObject(layout).FindProperty("walls");
        Assert.That(walls.arraySize, Is.EqualTo(4));
        for (int i = 0; i < walls.arraySize; i++) Assert.That(((BoxCollider2D)walls.GetArrayElementAtIndex(i).objectReferenceValue).isTrigger, Is.False);
        Assert.That(layout.transform.Find("NorthWall").localPosition.y, Is.EqualTo(12.25f));
        Assert.That(layout.transform.Find("SouthWall").localPosition.y, Is.EqualTo(-12.25f));
        Assert.That(layout.transform.Find("EastWall").localPosition.x, Is.EqualTo(16.25f));
        Assert.That(layout.transform.Find("WestWall").localPosition.x, Is.EqualTo(-16.25f));
    }

    [Test]
    public void Game_scene_uses_the_bounded_room_prefab()
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/Game.unity");
        try
        {
            RoomBounds room = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<RoomBounds>() != null) room = root.GetComponent<RoomBounds>();
            Assert.That(room, Is.Not.Null);
            Assert.That(room.Size, Is.EqualTo(new Vector2(32f, 24f)));
            Assert.That(room.GetComponentInChildren<RoomLayout>(), Is.Not.Null);
            Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(room), Is.Not.Null);
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    [TestCase(1f, 0.45f, 15.55f, 11.55f)]
    [TestCase(2.5f, 0.6f, 14.5f, 10.5f)]
    public void Body_clamping_handles_high_speed_and_teleports_including_scaled_bosses(float scale, float radius, float maxX, float maxY)
    {
        var roomObject = new GameObject("Room test");
        var bodyObject = new GameObject("Body test");
        try
        {
            var room = roomObject.AddComponent<RoomBounds>();
            var body = bodyObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            bodyObject.AddComponent<CircleCollider2D>().radius = radius;
            bodyObject.transform.localScale = Vector3.one * scale;
            body.position = new Vector2(maxX - 0.1f, maxY - 0.1f);
            body.linearVelocity = Vector2.one * 100f;
            room.Constrain(body);
            Vector2 next = body.position + body.linearVelocity * Time.fixedDeltaTime;
            Assert.That(next.x, Is.LessThanOrEqualTo(maxX + 0.001f));
            Assert.That(next.y, Is.LessThanOrEqualTo(maxY + 0.001f));
            body.position = new Vector2(100f, -100f);
            room.Constrain(body);
            Assert.That(Vector2.Distance(body.position, new Vector2(maxX, -maxY)), Is.LessThan(0.001f));
        }
        finally { Object.DestroyImmediate(bodyObject); Object.DestroyImmediate(roomObject); }
    }

    [Test]
    public void Camera_keeps_the_floor_visible_at_the_edge_and_in_a_very_wide_window()
    {
        var roomObject = new GameObject("Camera room test");
        var cameraObject = new GameObject("Camera test");
        var target = new GameObject("Camera target test");
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var room = roomObject.AddComponent<RoomBounds>();
        try
        {
            typeof(RoomBounds).GetMethod("Awake", flags).Invoke(room, null);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            var follow = cameraObject.AddComponent<CameraFollow>();
            follow.Target = target.transform;
            target.transform.position = new Vector3(100f, 100f, 0f);
            cameraObject.transform.position = new Vector3(100f, 100f, -10f);
            foreach (float aspect in new[] { 16f / 9f, 4f, 16f / 9f })
            {
                camera.aspect = aspect;
                typeof(CameraFollow).GetMethod("LateUpdate", flags).Invoke(follow, null);
                Vector2 half = new Vector2(camera.orthographicSize * aspect, camera.orthographicSize);
                Vector2 position = cameraObject.transform.position;
                Assert.That(position.x - half.x, Is.GreaterThanOrEqualTo(room.OuterArea.xMin - 0.001f));
                Assert.That(position.x + half.x, Is.LessThanOrEqualTo(room.OuterArea.xMax + 0.001f));
                Assert.That(position.y - half.y, Is.GreaterThanOrEqualTo(room.OuterArea.yMin - 0.001f));
                Assert.That(position.y + half.y, Is.LessThanOrEqualTo(room.OuterArea.yMax + 0.001f));
            }
            Assert.That(camera.orthographicSize, Is.EqualTo(6f));
        }
        finally
        {
            typeof(RoomBounds).GetMethod("OnDestroy", flags).Invoke(room, null);
            Object.DestroyImmediate(target); Object.DestroyImmediate(cameraObject); Object.DestroyImmediate(roomObject);
        }
    }

    [Test]
    public void Cat_slap_cannot_teleport_the_robot_through_a_wall()
    {
        var roomObject = new GameObject("Slap room test");
        var robot = new GameObject("Slap robot test");
        var cat = new GameObject("Slap cat test");
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var room = roomObject.AddComponent<RoomBounds>();
        try
        {
            typeof(RoomBounds).GetMethod("Awake", flags).Invoke(room, null);
            robot.AddComponent<CircleCollider2D>().radius = 0.45f;
            var body = robot.AddComponent<Rigidbody2D>();
            body.position = new Vector2(15.4f, 0f);
            robot.transform.position = new Vector3(15.4f, 0f, 0f);
            var stats = robot.AddComponent<PlayerStats>();
            typeof(PlayerStats).GetMethod("Awake", flags).Invoke(stats, null);
            var boss = cat.AddComponent<BossCat>();
            typeof(EnemyBase).GetMethod("Awake", flags).Invoke(boss, null);
            cat.transform.position = new Vector3(14f, 0f, 0f);
            typeof(EnemyBase).GetField("player", flags).SetValue(boss, robot.transform);
            typeof(BossCat).GetMethod("Slap", flags).Invoke(boss, null);
            Assert.That(Vector2.Distance(body.position, new Vector2(15.55f, 0f)), Is.LessThan(0.001f));
            Assert.That(stats.Battery, Is.EqualTo(85f));
        }
        finally
        {
            typeof(RoomBounds).GetMethod("OnDestroy", flags).Invoke(room, null);
            Object.DestroyImmediate(cat); Object.DestroyImmediate(robot); Object.DestroyImmediate(roomObject);
        }
    }
}
