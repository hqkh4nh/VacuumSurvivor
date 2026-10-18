using UnityEngine;

namespace VacuumSurvivor
{
    /// Assigns a code-generated circle sprite to this renderer at startup, so no art asset
    /// has to be imported or serialized into the scene.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProceduralSprite : MonoBehaviour
    {
        [SerializeField] Color color = Color.white;
        [SerializeField] bool eyes = true;
        [SerializeField] int pixels = 64;

        public Color Color { get => color; set => color = value; }
        public bool Eyes { get => eyes; set => eyes = value; }
        public int Pixels { get => pixels; set => pixels = value; }

        void Awake() => Apply();

        public void Apply()
        {
            GetComponent<SpriteRenderer>().sprite = SpriteFactory.CircleWithEyes(color, pixels, eyes);
        }
    }
}
