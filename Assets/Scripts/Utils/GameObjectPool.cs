using System.Collections.Generic;
using UnityEngine;

namespace VacuumSurvivor
{
    /// Reuses instances of one prefab instead of Instantiate/Destroy in the hot loop.
    public class GameObjectPool : MonoBehaviour
    {
        [SerializeField] GameObject prefab;
        [SerializeField] int initialSize = 32;

        readonly Queue<GameObject> idle = new Queue<GameObject>();

        public GameObject Prefab { get => prefab; set => prefab = value; }

        void Awake() => Prewarm();

        public void Prewarm()
        {
            if (prefab == null) return;
            while (idle.Count < initialSize) idle.Enqueue(CreateNew());
        }

        GameObject CreateNew()
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        public GameObject Get(Vector3 position)
        {
            var obj = idle.Count > 0 ? idle.Dequeue() : CreateNew();
            obj.transform.position = position;
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            idle.Enqueue(obj);
        }
    }
}
