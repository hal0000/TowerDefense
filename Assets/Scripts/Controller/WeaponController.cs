using TowerDefense.Core;
using TowerDefense.Interface;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class WeaponController : MonoBehaviourExtra
    {
        [HideInInspector] public float FireRate = 1f;
        [HideInInspector] public int Damage = 1;
        private static GameScene _scene;
        private float _cooldown;
        private Collider[] _hitBuffer;
        
        [Header("Fire Settings")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _bulletTravelTime = 0.2f;
        [SerializeField] private Transform _barrel;

        [Header("Targeting")]
        [SerializeField] private float _range = 5f;
        [SerializeField] private LayerMask _enemyLayer = ~0;
        [SerializeField] private int _maxTargets = 8;
        void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs) _scene = gs;
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
            Vector3 detectCenter = transform.position;
            int count = Physics.OverlapSphereNonAlloc(detectCenter, _range, _hitBuffer, _enemyLayer);
            if (count == 0) return;
            IEnemy target = null;
            for (int i = 0; i < count; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent<IEnemy>(out var ie)) continue;
                target = ie;
                break;
            }
            if (target == null) return;
            _barrel.LookAt(target.GetPosition(), Vector3.up);
            _barrel.Rotate(-90f, 0f, 0f, Space.Self);
            var bullet = _scene.BulletPool.GetBullet();
            bullet.Initialize(_firePoint, target, Damage,_bulletTravelTime);
            _cooldown = 1f / FireRate;
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