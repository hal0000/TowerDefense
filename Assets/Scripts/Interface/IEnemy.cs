using UnityEngine;

namespace TowerDefense.Interface
{
    public interface IEnemy
    {
        public void GetDamage(int value);
        public void Die();
        public void Win();
        public Vector3 GetPosition();
        public bool AmIAlive();
    }
}