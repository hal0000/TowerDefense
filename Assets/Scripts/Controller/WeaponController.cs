using TowerDefense.Animation;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class WeaponController : MonoBehaviourExtra
    {
        private static GameScene _scene;

        [Header("Fire Settings")] [SerializeField]
        private Transform _firePoint;

        [SerializeField] private int _maxTargets = 2;
        [SerializeField] private float _bulletTravelTime = 0.2f;
        [SerializeField] private Transform _barrel;
        public SphereCollider _col;
        [SerializeField] private LayerMask _enemyLayer = ~0;
        [SerializeField] private AlphaLoop _rangeIndicator;
        private float _cooldown;

        private int _damage = 1;
        private float _fireRate = 1f;
        private Collider[] _hitBuffer;
        private float _range = 5f;

        private void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs) _scene = gs;
            _hitBuffer = new Collider[_maxTargets];
        }

        protected override void OnEnable()
        {
            EventManager.OnGameStateChanged += GameStateChanged;
        }

        protected override void OnDisable()
        {
            EventManager.OnGameStateChanged -= GameStateChanged;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_firePoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _col.radius);
        }
#endif

        public void Initialize(TowerModel model)
        {
            _fireRate = model.FireRate;
            _damage = model.Damage;
            _range = model.Range;
            _col.radius = _range;
            _rangeIndicator.transform.localScale = new Vector3(_range, _range, 1);
        }

        private void GameStateChanged(Enums.GameState type)
        {
            switch (type)
            {
                case Enums.GameState.Preparing:
                case Enums.GameState.Editing:
                    _rangeIndicator.StartAnimation();
                    _col.enabled = false;
                    break;
                default:
                    _rangeIndicator.StopAnimation();
                    _col.enabled = true;
                    break;
            }
        }

        protected override void Tick()
        {
            Vector3 detectCenter = transform.position;
            int count = Physics.OverlapSphereNonAlloc(detectCenter, _col.radius, _hitBuffer, _enemyLayer);
            if (count == 0) return;
            IEnemy target = null;
            for (int i = 0; i < count; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent(out IEnemy ie)) continue;
                target = ie;
                break;
            }

            if (target == null) return;
            Vector3 dir = (target.GetPosition() - _barrel.position).normalized;
            _barrel.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            float dt = TimeManager.DeltaTime;
            if (_cooldown > 0f)
            {
                _cooldown -= dt;
                return;
            }

            BulletController bullet = _scene.BulletPool.GetBullet();
            bullet.Initialize(_firePoint, target, _damage, _bulletTravelTime);
            _cooldown = 1f / _fireRate;
        }
    }
}