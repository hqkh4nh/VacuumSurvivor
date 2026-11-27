using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VacuumSurvivor;

public class AuthoredRoomTests
{
    [TestCase("LivingRoom", "LivingFloor", 32f, 24f)]
    [TestCase("Bedroom", "BedroomFloor", 22f, 18f)]
    [TestCase("Kitchen", "KitchenFloor", 34f, 18f)]
    public void Rooms_have_authored_art_walls_and_furniture(string name, string floorName, float width, float height)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Maps/Rooms/" + name + ".prefab");
        Assert.That(prefab, Is.Not.Null);
        var layout = prefab.GetComponent("RoomLayout");
        Assert.That(layout, Is.Not.Null);
        var so = new SerializedObject(layout);
        Assert.That(so.FindProperty("size").vector2Value, Is.EqualTo(new Vector2(width, height)));
        var floor = so.FindProperty("floor").objectReferenceValue as SpriteRenderer;
        Assert.That(floor, Is.Not.Null);
        Assert.That(floor.sprite, Is.SameAs(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Soft2D/Rooms/" + floorName + ".png")));
        Assert.That(so.FindProperty("walls").arraySize, Is.EqualTo(4));
        Assert.That(so.FindProperty("obstacles").arraySize, Is.GreaterThanOrEqualTo(3));
        foreach (var renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true)) Assert.That(renderer.sprite, Is.Not.Null, renderer.name);
        foreach (var collider in prefab.GetComponentsInChildren<BoxCollider2D>(true)) Assert.That(collider.isTrigger, Is.False);
        Assert.That(prefab.GetComponentInChildren<ProceduralSprite>(true), Is.Null);
        foreach (var script in prefab.GetComponentsInChildren<MonoBehaviour>(true)) Assert.That(script, Is.Not.Null, "Missing script in authored room");
    }

    [Test]
    public void Changing_rooms_only_selects_existing_layouts()
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Maps/Room.prefab");
        try
        {
            var room = root.GetComponent<RoomBounds>();
            Assert.That(root.GetComponent<SpriteRenderer>(), Is.Null, "Container must not retain the old tiled floor");
            var layouts = new SerializedObject(room).FindProperty("layouts");
            Assert.That(layouts, Is.Not.Null);
            Assert.That(layouts.arraySize, Is.EqualTo(3));
            int objects = root.GetComponentsInChildren<Transform>(true).Length;
            Vector2[] dimensions = { new Vector2(32, 24), new Vector2(22, 18), new Vector2(34, 18) };
            for (int index = 0; index < 3; index++)
            {
                typeof(RoomBounds).GetMethod("SelectLayout").Invoke(room, new object[] { index });
                Assert.That(room.Size, Is.EqualTo(dimensions[index]));
                int active = 0;
                for (int i = 0; i < 3; i++)
                {
                    var layout = (Component)layouts.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (layout.gameObject.activeSelf) active++;
                    Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(layout), Is.Not.Null);
                }
                Assert.That(active, Is.EqualTo(1));
                Assert.That(root.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(objects));
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    [TestCase(0, 0.45f)] [TestCase(1, 0.45f)] [TestCase(2, 0.45f)]
    [TestCase(0, 1.5f)] [TestCase(1, 1.5f)] [TestCase(2, 1.5f)]
    public void Robot_and_boss_can_reach_every_open_grid_cell_and_spawn_outside_furniture(int index, float radius)
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Maps/Room.prefab");
        try
        {
            var room = root.GetComponent<RoomBounds>();
            Assert.That(root.GetComponent<SpriteRenderer>(), Is.Null, "Container must not retain the old tiled floor");
            var layouts = new SerializedObject(room).FindProperty("layouts");
            Assert.That(layouts, Is.Not.Null);
            typeof(RoomBounds).GetMethod("SelectLayout").Invoke(room, new object[] { index });
            Physics2D.SyncTransforms();
            var layout = (Component)layouts.GetArrayElementAtIndex(index).objectReferenceValue;
            var obstacles = new SerializedObject(layout).FindProperty("obstacles");
            var blockers = new List<Bounds>();
            for (int i = 0; i < obstacles.arraySize; i++) blockers.Add(((BoxCollider2D)obstacles.GetArrayElementAtIndex(i).objectReferenceValue).bounds);
            bool Blocked(Vector2 p)
            {
                foreach (var bounds in blockers)
                    if (p.x > bounds.min.x - radius - 0.15f && p.x < bounds.max.x + radius + 0.15f &&
                        p.y > bounds.min.y - radius - 0.15f && p.y < bounds.max.y + radius + 0.15f) return true;
                return false;
            }
            foreach (var bounds in blockers)
            {
                Vector2 clear = room.ClearPosition(bounds.center, radius);
                Assert.That(Blocked(clear), Is.False, "Spawn remained inside furniture");
                Assert.That(clear.x, Is.InRange(room.Area.xMin + radius, room.Area.xMax - radius));
                Assert.That(clear.y, Is.InRange(room.Area.yMin + radius, room.Area.yMax - radius));
            }
            const float step = 0.5f;
            int columns = Mathf.FloorToInt((room.Area.width - radius * 2) / step) + 1;
            int rows = Mathf.FloorToInt((room.Area.height - radius * 2) / step) + 1;
            var open = new HashSet<Vector2Int>();
            Vector2Int start = default;
            float nearest = float.PositiveInfinity;
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                Vector2 point = room.Area.min + Vector2.one * radius + new Vector2(x, y) * step;
                if (Blocked(point)) continue;
                var cell = new Vector2Int(x, y);
                open.Add(cell);
                if (point.sqrMagnitude < nearest) { nearest = point.sqrMagnitude; start = cell; }
            }
            Assert.That(open.Count, Is.GreaterThan(0));
            var visited = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>(); queue.Enqueue(start);
            Vector2Int[] moves = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var move in moves)
                {
                    var next = cell + move;
                    if (open.Contains(next) && visited.Add(next)) queue.Enqueue(next);
                }
            }
            Assert.That(visited.Count, Is.EqualTo(open.Count), "Furniture creates unreachable floor pockets");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
