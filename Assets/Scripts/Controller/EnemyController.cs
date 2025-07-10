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
            if (!_flaggedForRecycle && _waypointIndex >= _waypoints.Count)
            {
                _flaggedForRecycle = true;
                Win();
                return;
            }

            var t = transform;
            Vector3 pos = t.position;
            Vector3 target = _waypoints[_waypointIndex];
            Vector3 delta = target - pos;
            float sqrMag = delta.sqrMagnitude;
            if (sqrMag < 0.001f)
            {
                _waypointIndex++;
                return;
            }

            float invMag = TypeExtensions.InvSqrt(sqrMag);
            Vector3 dir = delta * invMag;
            float step = Model.Speed *  TimeManager.DeltaTime;
            pos += dir * step;
            t.forward = dir;
            t.position = pos;

            if ((target - pos).sqrMagnitude < 0.0001f)
                _waypointIndex++;
        }

        public void ApplyDamage(int amount)
        {
            Model.Health -= amount;
            if (Model.Health <= 0) Die();
        }

        public void GetDamage() { }
        public void ApplyDamage()
        {
            
        }

        public void Win()
        {
            _scene.EnemyPool.ReturnEnemy(Model.Type, this);
        }

        public void Die()
        {
            _scene.EnemyPool.ReturnEnemy(Model.Type, this);
        }

        private void OnTriggerEnter(Collider other)
        {
            // if (other.CompareTag("Goal"))
            // {
            // Win();
            // Die();
            // }
        }
    }
}