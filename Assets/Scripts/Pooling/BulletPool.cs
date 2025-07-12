using System.Collections.Generic;
using TowerDefense.Controller;
using UnityEngine;

namespace TowerDefense.Pooling
{
    public sealed class BulletPool : MonoBehaviour
    {
        [SerializeField] private BulletController _prefab;
        [SerializeField] private int _poolSize = 20;

        private Queue<BulletController> _pool;

        private void Awake()
        {
            _pool = new Queue<BulletController>(_poolSize);
            for (int i = 0; i < _poolSize; i++)
            {
                BulletController b = Instantiate(_prefab, transform);
                b.gameObject.SetActive(false);
                _pool.Enqueue(b);
            }
        }

        public BulletController GetBullet()
        {
            BulletController b;
            b = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_prefab, transform);
            b.OnSpawn();
            b.gameObject.SetActive(true);
            return b;
        }

        public void ReturnBullet(BulletController bullet)
        {
            bullet.gameObject.SetActive(false);
            _pool.Enqueue(bullet);
        }
    }
}