using TowerDefense.Core;
using TowerDefense.Interface;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class BulletController : MonoBehaviourExtra, IPoolable
    {
        private static GameScene _scene;
        private int _damage;
        private float _duration = 0.2f;
        private float _elapsed;
        private bool _initialized;
        private Vector3 _startPos;

        private IEnemy _target;

        public void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs) _scene = gs;
        }

        public void OnSpawn()
        {
            _elapsed = 0f;
            _initialized = true;
        }

        public void Initialize(Transform start, IEnemy target, int damage, float duration = 0.2f)
        {
            _startPos = start.position;
            transform.position = _startPos;
            _target = target;
            _duration = duration;
            _damage = damage;
        }

        protected override void Tick()
        {
            if (!_initialized || _target == null) return;

            _elapsed += TimeManager.DeltaTime;
            float t = _elapsed / _duration;
            if (t >= 1f)
            {
                transform.position = _target.GetPosition();
                _target.GetDamage(_damage);
                ReturnBullet();
            }
            else
            {
                transform.position = Vector3.Lerp(_startPos, _target.GetPosition(), t);
            }
        }

        private void ReturnBullet()
        {
            _initialized = false;
            _target = null;
            _scene.BulletPool.ReturnBullet(this);
        }
    }
}