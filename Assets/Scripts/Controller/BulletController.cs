using TowerDefense.Core;
using TowerDefense.Interface;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class BulletController : MonoBehaviourExtra, IPoolable
    {
        private static GameScene _scene;

        Transform _target;
        Vector3 _startPos;
        float _duration = 0.2f;
        float _elapsed;
        bool _initialized;
        
        public void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs) _scene = gs;
        }

        public void Initialize(Transform start, Transform target, float duration = 0.2f)
        {
            _startPos = start.position;
            transform.position = _startPos;
            _target = target;
            _duration = duration;

        }

        protected override void Tick()
        {
            if (!_initialized || _target == null)
            {
                return;
            }

            _elapsed += TimeManager.DeltaTime;
            float t = _elapsed / _duration;
            if (t >= 1f)
            {
                transform.position = _target.position;
                ReturnBullet();
            }
            else
            {
                transform.position = Vector3.Lerp(_startPos, _target.position, t);
            }
        }

        void ReturnBullet()
        {
            _initialized = false;
            _target = null;
            _scene.BulletPool.ReturnBullet(this);
        }

        public void OnSpawn()
        {
            _elapsed = 0f;
            _initialized = true;
        }
    }
}