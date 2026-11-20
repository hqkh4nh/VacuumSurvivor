using UnityEngine;

namespace VacuumSurvivor
{
    /// Sizes the tiled floor, preserving authored art or generating a fallback when none is assigned.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProceduralFloor : MonoBehaviour
    {
        [SerializeField] int pixels = 256;
        [SerializeField] Vector2 area = new Vector2(32f, 24f);

        void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null) sr.sprite = SpriteFactory.FloorTile(pixels);
            sr.drawMode = SpriteDrawMode.Tiled;
            var room = GetComponent<RoomBounds>();
            sr.size = room != null ? room.Size : area;
        }
    }
}
