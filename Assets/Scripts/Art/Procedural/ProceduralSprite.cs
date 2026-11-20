using UnityEngine;

namespace VacuumSurvivor
{
    /// Assigns a code-generated sprite to this renderer at startup, so no art asset has to be
    /// imported or serialized into the scene. The kind picks which silhouette to build.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProceduralSprite : MonoBehaviour
    {
        [SerializeField] SpriteKind kind = SpriteKind.DustBunny;
        [SerializeField] Color color = Color.white;
        [SerializeField] int pixels = 64;

        public SpriteKind Kind { get => kind; set => kind = value; }
        public Color Color { get => color; set => color = value; }
        public int Pixels { get => pixels; set => pixels = value; }

        void Awake() => Apply();

        public void Apply()
        {
            GetComponent<SpriteRenderer>().sprite = SpriteFactory.Build(kind, color, pixels);
        }
    }
}
