using TowerDefense.Core;
using TowerDefense.Interface;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class WeaponController : MonoBehaviourExtra
    {
        private static GameScene _scene;
        private float _cooldown;
        private Collider[] _hitBuffer;

        [Header("Fire Settings")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _fireRate = 1f;
        [SerializeField] private float _bulletTravelTime = 0.2f;
        [SerializeField] private Transform _barrel;

        [Header("Targeting")]
        [SerializeField] private float _range = 5f;
        [SerializeField] private LayerMask _enemyLayer = ~0;
        [SerializeField] private int _maxTargets = 8;
        void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs)
                _scene = gs;

            _hitBuffer = new Collider[_maxTargets];
        }
        
        protected override void Tick()
        {
            float dt = TimeManager.DeltaTime;
            if (_cooldown > 0f)
            {
                _cooldown -= dt;
                return;
            }
            // non-alloc sphere overlap
            int count = Physics.OverlapSphereNonAlloc(_firePoint.position, _range, _hitBuffer, _enemyLayer);

            if (count == 0) 
                return;

            // pick the first valid enemy
            Transform target = null;
            for (int i = 0; i < count; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent<IEnemy>(out var ie)) continue;
                ie.GetDamage();
                target = col.transform;
                break;
            }
            if (target == null) return;
            _barrel.LookAt(target.position, Vector3.up);
            _barrel.Rotate(-90f, 0f, 0f, Space.Self);
            // fire bullet
            var bullet = _scene.BulletPool.GetBullet();
            bullet.Initialize(_firePoint, target.transform, _bulletTravelTime);
            _cooldown = 1f / _fireRate;
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (_firePoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, _range);
        }
#endif
    }
}