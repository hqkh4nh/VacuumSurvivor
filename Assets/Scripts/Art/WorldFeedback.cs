using UnityEngine;

namespace VacuumSurvivor
{
    // Slices the PixelFX exports and shares a bounded set of renderers for a scene.
    internal sealed class WorldFeedback : MonoBehaviour
    {
        const int FramesPerSecond = 20;
        static WorldFeedback instance;

        readonly Slot[] slots = new Slot[24];
        Sprite[] hitFrames;
        Sprite[] pickupFrames;

        sealed class Slot
        {
            public SpriteRenderer Art;
            public Sprite[] Frames;
            public float Age;
            public float Speed;
            public int Frame;
        }

        public static void Hit(Vector3 position, Vector2 direction, Color color, float scale)
        {
            if (!Application.isPlaying) return;
            var effects = GetInstance();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            effects.Play(effects.hitFrames, position, color, scale, angle, 1.5f);
        }

        public static void Pickup(Vector3 position, Color color)
        {
            if (!Application.isPlaying) return;
            var effects = GetInstance();
            effects.Play(effects.pickupFrames, position, color, 1.4f, 0f, 1.9f);
        }

        static WorldFeedback GetInstance()
        {
            if (instance == null)
                instance = new GameObject("World Feedback").AddComponent<WorldFeedback>();
            return instance;
        }

        void Awake()
        {
            hitFrames = LoadFrames("hit-spark", 136, 80, 3, 11, new Vector2(44f / 136f, 16f / 80f));
            // The burst starts 30 pixels above the original ground pivot.
            pickupFrames = LoadFrames("collect-burst", 112, 108, 6, 28, new Vector2(0.5f, 44f / 108f));
        }

        static Sprite[] LoadFrames(string name, int width, int height, int columns, int count, Vector2 pivot)
        {
            var atlas = Resources.Load<Texture2D>("Presentation/" + name);
            if (atlas == null || atlas.width != columns * width || atlas.height < (count + columns - 1) / columns * height)
                return null;

            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                var frame = new Rect(i % columns * width, atlas.height - (i / columns + 1) * height, width, height);
                frames[i] = Sprite.Create(atlas, frame, pivot, 80f, 0, SpriteMeshType.FullRect);
            }
            return frames;
        }

        void Play(Sprite[] frames, Vector3 position, Color color, float scale, float angle, float speed)
        {
            if (frames == null) return;

            // Reserve the first slots for pickups. Dense combat can drop incidental sparks.
            int first = frames == pickupFrames ? 0 : 4;
            int last = first == 0 ? 4 : slots.Length;
            for (int i = first; i < last; i++)
            {
                if (slots[i]?.Frames != null) continue;
                if (slots[i] == null)
                {
                    var art = new GameObject("Effect").AddComponent<SpriteRenderer>();
                    art.transform.SetParent(transform, false);
                    art.sortingOrder = 30;
                    slots[i] = new Slot { Art = art };
                }

                var slot = slots[i];
                slot.Art.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
                slot.Art.transform.localScale = Vector3.one * scale;
                slot.Art.color = color;
                slot.Art.sprite = frames[0];
                slot.Art.enabled = true;
                slot.Frames = frames;
                slot.Frame = 0;
                slot.Age = 0f;
                slot.Speed = speed;
                return;
            }
        }

        void Update()
        {
            foreach (var slot in slots)
            {
                if (slot?.Frames == null) continue;
                slot.Age += Time.deltaTime * slot.Speed;
                int frame = (int)(slot.Age * FramesPerSecond);
                if (frame >= slot.Frames.Length)
                {
                    slot.Art.enabled = false;
                    slot.Frames = null;
                }
                else if (frame != slot.Frame)
                {
                    slot.Art.sprite = slot.Frames[frame];
                    slot.Frame = frame;
                }
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            DestroyFrames(hitFrames);
            DestroyFrames(pickupFrames);
        }

        static void DestroyFrames(Sprite[] frames)
        {
            if (frames == null) return;
            foreach (var frame in frames) Destroy(frame);
        }
    }
}
