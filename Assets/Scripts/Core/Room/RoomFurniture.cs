using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    // Furniture hugs the walls so the middle of each arena stays open for hordes and bosses.
    internal sealed class RoomFurniture : MonoBehaviour
    {
        readonly List<Rect> footprints = new List<Rect>();
        RoomBounds room;
        Sprite surface;

        public static RoomFurniture Build(RoomBounds room, int index)
        {
            var wall = room.transform.Find("NorthWall");
            if (wall == null || !wall.TryGetComponent<SpriteRenderer>(out var renderer)) return null;

            var layout = new GameObject("Room furniture").AddComponent<RoomFurniture>();
            layout.transform.SetParent(room.transform, false);
            layout.room = room;
            layout.surface = renderer.sprite;
            layout.Furnish(index);
            return layout;
        }

        void Furnish(int index)
        {
            float top = room.Size.y * 0.5f;
            float left = -room.Size.x * 0.5f;
            float right = -left;

            if (index == 0)
            {
                Rug(new Vector2(0f, top - 4.9f), new Vector2(8.4f, 5.8f), new Color32(78, 112, 105, 255));
                var sofa = Piece("Sofa", new Vector2(0f, top - 1.1f), new Vector2(6.8f, 2.2f), new Color32(65, 105, 112, 255));
                Inset(sofa, "Backrest", new Vector2(0f, 0.81f), new Vector2(6.5f, 0.43f));
                for (int i = -1; i <= 1; i++)
                    Inset(sofa, "Cushion", new Vector2(i * 1.85f, -0.22f), new Vector2(1.7f, 1.12f));
                Piece("Media console", new Vector2(left + 0.75f, -3f), new Vector2(1.5f, 4f), new Color32(106, 77, 63, 255));
            }
            else if (index == 1)
            {
                Rug(new Vector2(-4.6f, top - 5.9f), new Vector2(6f, 3.4f), new Color32(99, 79, 111, 255));
                var bed = Piece("Bed", new Vector2(-4.6f, top - 2.25f), new Vector2(4.2f, 4.5f), new Color32(95, 78, 125, 255));
                Inset(bed, "Blanket", new Vector2(0f, -0.74f), new Vector2(3.85f, 2.55f));
                Inset(bed, "Pillows", new Vector2(0f, 1.24f), new Vector2(3.3f, 0.85f), new Color32(183, 171, 192, 255));
                var wardrobe = Piece("Wardrobe", new Vector2(right - 1f, 3.5f), new Vector2(2f, 5f), new Color32(95, 66, 70, 255));
                Inset(wardrobe, "Doors", Vector2.zero, new Vector2(1.62f, 4.65f));
            }
            else
            {
                AddTileSeams();
                var counter = Piece("Kitchen counter", new Vector2(2f, top - 1.35f), new Vector2(12f, 2.7f), new Color32(73, 97, 101, 255));
                Inset(counter, "Worktop", Vector2.zero, new Vector2(11.65f, 2.3f));
                Inset(counter, "Sink", new Vector2(3.6f, 0f), new Vector2(2.65f, 1.65f), new Color32(62, 88, 109, 255), 3);
                for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    Inset(counter, "Hob", new Vector2(-3.9f + x, -0.48f + y * 0.96f), new Vector2(0.71f, 0.71f), new Color32(43, 52, 63, 255), 3);
                var fridge = Piece("Fridge", new Vector2(left + 1.35f, 1.9f), new Vector2(2.7f, 3.4f), new Color32(142, 160, 163, 255));
                Inset(fridge, "Handle", new Vector2(0.93f, 0.43f), new Vector2(0.17f, 1.25f), new Color32(59, 78, 87, 255));
                Rug(new Vector2(2f, top - 3.45f), new Vector2(8f, 1.1f), new Color32(92, 121, 109, 255));
            }
        }

        void AddTileSeams()
        {
            var grout = new Color32(28, 43, 47, 115);
            Vector2 size = room.Size;
            Surface(transform, "Tile surface", Vector2.zero, size, new Color32(104, 143, 145, 164), -81);
            for (float x = -size.x * 0.5f + 2f; x < size.x * 0.5f; x += 2f)
                Surface(transform, "Vertical grout", new Vector2(x, 0f), new Vector2(0.055f, size.y), grout, -80);
            for (float y = -size.y * 0.5f + 2f; y < size.y * 0.5f; y += 2f)
                Surface(transform, "Horizontal grout", new Vector2(0f, y), new Vector2(size.x, 0.055f), grout, -80);
        }

        void Rug(Vector2 position, Vector2 size, Color color)
        {
            var rug = Surface(transform, "Rug", position, size, color * new Color(0.65f, 0.65f, 0.65f, 1f), -8);
            Surface(rug.transform, "Weave", Vector2.zero, size - Vector2.one * 0.24f, color, -7);
        }

        SpriteRenderer Piece(string name, Vector2 position, Vector2 size, Color color)
        {
            var piece = Surface(transform, name, position, size, color, 0);
            var box = piece.gameObject.AddComponent<BoxCollider2D>();
            box.size = size;
            footprints.Add(new Rect((Vector2)piece.transform.position - size * 0.5f, size));
            return piece;
        }

        void Inset(SpriteRenderer piece, string name, Vector2 position, Vector2 size, Color? color = null, int order = 1)
        {
            Surface(piece.transform, name, position, size, color ?? Color.Lerp(piece.color, Color.white, 0.2f), order);
        }

        SpriteRenderer Surface(Transform parent, string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = surface;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        public Vector2 ClearPosition(Vector2 position, float radius)
        {
            // The authored footprints are spaced apart and leave an open central route.
            foreach (var footprint in footprints)
            {
                float margin = radius + 0.15f;
                var blocked = Rect.MinMaxRect(footprint.xMin - margin, footprint.yMin - margin,
                    footprint.xMax + margin, footprint.yMax + margin);
                if (!blocked.Contains(position)) continue;

                var top = new Vector2(position.x, blocked.yMax);
                var bottom = new Vector2(position.x, blocked.yMin);
                var left = new Vector2(blocked.xMin, position.y);
                var right = new Vector2(blocked.xMax, position.y);
                Vector2 nearest = room.Area.center;
                float distance = (nearest - position).sqrMagnitude;
                Check(top);
                Check(bottom);
                Check(left);
                Check(right);
                position = nearest;

                void Check(Vector2 candidate)
                {
                    if (room.Clamp(candidate, radius) != candidate) return;
                    float candidateDistance = (candidate - position).sqrMagnitude;
                    if (candidateDistance >= distance) return;
                    nearest = candidate;
                    distance = candidateDistance;
                }
            }
            return position;
        }
    }
}
