using UnityEngine;

namespace VacuumSurvivor
{
    /// Fills the play area with a generated, tiled parquet floor at startup, so the scene has ground
    /// to stand on without any imported texture.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProceduralFloor : MonoBehaviour
    {
        [SerializeField] int pixels = 256;
        [SerializeField] Vector2 area = new Vector2(60f, 60f);

        void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.FloorTile(pixels);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = area;
        }
    }
}
