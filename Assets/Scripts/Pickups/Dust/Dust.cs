using UnityEngine;

namespace VacuumSurvivor
{
    /// A dropped dust mote (the game's XP). The player's magnet pulls it in; on pickup it returns
    /// to its pool rather than being destroyed.
    public class Dust : MonoBehaviour
    {
        public int Value { get; private set; } = 1;
        public GameObjectPool Pool { get; set; }

        float pullSpeed;

        public void Init(int value)
        {
            Value = value;
            pullSpeed = 0f;
        }

        public void PullToward(Vector3 target)
        {
            pullSpeed += Time.deltaTime * 14f;   // accelerate the longer it is being pulled
            transform.position = Vector3.MoveTowards(transform.position, target, pullSpeed * Time.deltaTime);
        }

        public void ReturnToPool()
        {
            if (Pool != null) Pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
