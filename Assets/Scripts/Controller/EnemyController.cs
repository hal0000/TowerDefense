using System;
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
        public EnemyModel Model { get; private set; }
        private IReadOnlyList<Vector3> _waypoints;
        private int _waypointIndex;
        private bool _flaggedForRecycle;
        public void Awake()
        {
            if (_scene == null && GameManager.Instance.CurrentScene is GameScene gs) _scene = gs;
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
            // 1) have we reached the end?
            var wp = _waypoints;
            int count = wp.Count;
            if (_waypointIndex >= count)
            {
                Win();
                return;
            }

            // 2) cache transform + positions
            Transform t = transform;
            Vector3 pos = t.position;
            Vector3 target = wp[_waypointIndex];

            // 3) delta + distance²
            Vector3 delta = target - pos;
            float sqrMag = delta.sqrMagnitude;
            if (sqrMag < 0.0001f)
            {
                _waypointIndex++;
                return;
            }

            // 4) build direction via fast inv-sqrt
            float invMag = TypeExtensions.InvSqrt(sqrMag);
            Vector3 dir = delta * invMag;

            // 5) compute how far we want to step this frame
            float step = Model.Speed * TimeManager.DeltaTime;
            float stepSqr = step * step;

            // 6) clamp to not overshoot:
            // if step would carry us beyond the waypoint, snap to it
            if (stepSqr >= sqrMag)
            {
                pos = target;
                _waypointIndex++;
            }
            else
            {
                pos += dir * step;
            }

            // 7) apply move + face direction
            t.position = pos;
            t.forward = dir;
        }
        

        public void GetDamage(int value)
        {
            Model.Health -= value;
            if (Model.Health <= 0) Die();
            
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
            EventManager.PlayerDidSomething(Enums.PlayerActions.GetDamage,Model.Damage);
            _scene.EnemyPool.ReturnEnemy(Model.Type, this);
        }

        public void Die()
        {
            EventManager.PlayerDidSomething(Enums.PlayerActions.EnemyKilled,Model.Gold);
            _scene.EnemyPool.ReturnEnemy(Model.Type, this);
        }
    }
}