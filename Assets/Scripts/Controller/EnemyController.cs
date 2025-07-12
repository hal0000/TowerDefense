using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class EnemyController : MonoBehaviourExtra, IEnemy, IPoolable
    {
        private static GameScene _scene;
        private bool _flaggedForRecycle;
        private int _waypointIndex;
        private IReadOnlyList<Vector3> _waypoints;
        public EnemyModel Model { get; private set; }

        public void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs) _scene = gs;
        }


        public void GetDamage(int value)
        {
            Model.Health -= value;
            if (Model.Health > 0) return;
            Die();
            _flaggedForRecycle = true;
        }

        public void ApplyDamage()
        {
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void Win()
        {
            if (_flaggedForRecycle) return;
            EventManager.PlayerDidSomething(Enums.PlayerActions.GetDamage, Model.Damage);
            _scene.EnemyPool.ReturnEnemy(Model.Type, this);
        }

        public void Die()
        {
            if (_flaggedForRecycle) return;
            EventManager.PlayerDidSomething(Enums.PlayerActions.EnemyKilled, Model.Gold);
            _scene.EnemyPool.ReturnEnemy(Model.Type, this);
        }

        public void OnSpawn()
        {
            _flaggedForRecycle = false;
            _waypointIndex = 0;
            if (_waypoints != null && _waypoints.Count > 0) transform.position = _waypoints[0];
        }

        public void Initialize(EnemyModel model, IReadOnlyList<Vector3> waypoints)
        {
            Model = model;
            _waypoints = waypoints;
        }

        protected override void Tick()
        {
            IReadOnlyList<Vector3> wp = _waypoints;
            int count = wp.Count;
            if (_waypointIndex >= count)
            {
                Win();
                _flaggedForRecycle = true;
                return;
            }
            Transform t = transform;
            Vector3 pos = t.position;
            Vector3 target = wp[_waypointIndex];

            Vector3 delta = target - pos;
            float sqrMag = delta.sqrMagnitude;
            if (sqrMag < 0.0001f)
            {
                _waypointIndex++;
                return;
            }
            float invMag = TypeExtensions.InvSqrt(sqrMag);
            Vector3 dir = delta * invMag;
            float step = Model.Speed * TimeManager.DeltaTime;
            float stepSqr = step * step;
            if (stepSqr >= sqrMag)
            {
                pos = target;
                _waypointIndex++;
            }
            else
            {
                pos += dir * step;
            }
            t.position = pos;
            t.forward = dir;
        }
    }
}