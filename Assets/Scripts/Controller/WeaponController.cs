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

        [Header("Fire Settings")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _barrel;
        [SerializeField] private AlphaLoop _rangeIndicator;
        [SerializeField] private LayerMask _enemyLayer = ~0;

        [SerializeField] private float _bulletTravelTime = 0.2f;
        private float _range = 5f;
        private float _fireRate = 1f;
        private int _damage = 1;

        private Collider[] _hitBuffer;
        private IEnemy _target;
        private float _cooldown;

        private void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs)
                _scene = gs;
            _hitBuffer = new Collider[32];
        }

        public void Initialize(TowerModel model)
        {
            _fireRate = model.FireRate;
            _damage = model.Damage;
            _range = model.Range;
            _rangeIndicator.transform.localScale = new Vector3(_range, _range, 1f);
        }
        protected override void OnEnable() => EventManager.OnGameStateChanged += GameStateChanged;
        protected override void OnDisable() => EventManager.OnGameStateChanged -= GameStateChanged;

        private void GameStateChanged(Enums.GameState state)
        {
            switch (state)
            {
                case Enums.GameState.Preparing:
                case Enums.GameState.Editing:
                    _rangeIndicator.StartAnimation();
                    break;
                default:
                    _rangeIndicator.StopAnimation();
                    break;
            }
        }

        protected override void Tick()
        {
            float dt = TimeManager.DeltaTime;
            if (TrackTarget())
            {
                Attack(_target, dt);
                return;
            }
            if (AcquireTarget())
            {
                Attack(_target, dt);
            }
        }

        private bool TrackTarget()
        {
            if (_target == null || !_target.AmIAlive()) return false;
            var pos = transform.position;
            var tpos = _target.GetPosition();
            float dx = pos.x - tpos.x;
            float dz = pos.z - tpos.z;
            float buffer = 1.125f;
            float r = _range + buffer;
            if (dx*dx + dz*dz <= r*r) return true;
            _target = null;
            return false;
        }

        private bool AcquireTarget()
        {
            Vector3 a = transform.position;
            Vector3 b = a; b.y += 2f;
            int hits = Physics.OverlapCapsuleNonAlloc(a, b, _range, _hitBuffer, _enemyLayer);
            for (int i = 0; i < hits; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent<IEnemy>(out var ie)) continue;
                if (!ie.AmIAlive()) continue;
                _target = ie;
                return true;
            }
            _target = null;
            return false;
        }

        private void Attack(IEnemy target, float dt)
        {
            Vector3 dir = (target.GetPosition() - _barrel.position).normalized;
            _barrel.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            if (_cooldown > 0f)
            {
                _cooldown -= dt;
                return;
            }
            var bullet = _scene.BulletPool.GetBullet();
            bullet.Initialize(_firePoint, target, _damage, _bulletTravelTime);
            _cooldown = 1f / _fireRate;
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 position = transform.position;
            position.y += 0.01f;
            Gizmos.DrawWireSphere(position, _range);
            if (_target != null)
            {
                Gizmos.DrawLine(position, _target.GetPosition());
            }
        }
#endif
    }
}