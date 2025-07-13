using System.Collections.Generic;
using PrimeTween;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.Pooling;
using UnityEngine;

namespace TowerDefense.Controller
{
    public abstract class BaseEnemyController : MonoBehaviourExtra, IEnemy, IPoolable
    {
        private static EnemyPool _enemyPool;
        public RectTransform HpBarRect;
        public float MaxHPBarWidth = 2f;
        public Transform Mesh;
        private int _maxHP;
        protected bool FlaggedForRecycle;
        protected int WaypointIndex;
        protected EnemyModel Model { get; private set; }
        protected IReadOnlyList<Vector3> Waypoints { get; private set; }

        private void Awake()
        {
            if (_enemyPool == null && GameManager.Instance.CurrentScene is GameScene gs)
                _enemyPool = gs.EnemyPool;
        }

        public void Win()
        {
            if (FlaggedForRecycle) return;
            FlaggedForRecycle = true;
            EventManager.PlayerDidSomething(Enums.PlayerActions.GetDamage, Model.Damage);
            _enemyPool.ReturnEnemy(Model.Type, this);
        }

        public void Die()
        {
            if (FlaggedForRecycle) return;
            FlaggedForRecycle = true;
            EventManager.PlayerDidSomething(Enums.PlayerActions.EnemyKilled, Model.Gold);
            _enemyPool.ReturnEnemy(Model.Type, this);
        }

        public void GetDamage(int value)
        {
            Model.Health = Mathf.Max(Model.Health - value, 0);
            UpdateHPBar(Model.Health, _maxHP);
            if (Model.Health <= 0 && !FlaggedForRecycle)
                Die();
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public bool AmIAlive() => !FlaggedForRecycle;

        public virtual void OnSpawn()
        {
            Model.Health = _maxHP;
            UpdateHPBar(_maxHP, _maxHP);
            FlaggedForRecycle = false;
            if (WaypointIndex < 0 || WaypointIndex >= Waypoints.Count) WaypointIndex = 0;
            transform.position = Waypoints[0];
        }

        public virtual void Initialize(EnemyModel model, IReadOnlyList<Vector3> waypoints)
        {
            Model = model;
            Waypoints = waypoints;
            _maxHP = model.Health;
        }

        protected override void Tick()
        {
            if (FlaggedForRecycle) return;
            Vector3 target;
            if (!GetCurrentTarget(out target))
            {
                Win();
                return;
            }

            bool arrived = StepTowards(target, GetMaintainY());
            if (arrived) OnArrived();
        }

        /// <summary>
        ///     Moves toward to target by Model.Speed * t. Returns true if we've reached/past it this frame.
        /// </summary>
        private bool StepTowards(Vector3 target, float maintainY)
        {
            // Clamp both ends to the desired Y
            target.y = maintainY;
            Vector3 start = transform.position;
            start.y = maintainY;

            // Compute horizontal delta only
            Vector3 delta = target - start;
            float sqrMag = delta.sqrMagnitude;
            if (sqrMag < 0.0001f) return true;

            // Normalize via fast InvSqrt
            float invMag = TypeExtensions.InvSqrt(sqrMag);
            Vector3 dir = delta * invMag;

            // Advance by speed * dt, but don’t overshoot
            float step = Model.Speed * TimeManager.DeltaTime;
            float stepSqr = step * step;
            if (stepSqr >= sqrMag)
            {
                // Snap to exact target
                transform.position = target;
                Mesh.forward = dir;
                return true;
            }

            // Partial move
            Vector3 next = start + dir * step;
            next.y = maintainY;
            transform.position = next;
            Mesh.forward = dir;
            return false;
        }

        /// <summary>
        ///     Return the current target; if out of waypoints, return false.
        /// </summary>
        protected abstract bool GetCurrentTarget(out Vector3 target);

        /// <summary>
        ///     Called after StepTowards returned true: bump waypoint index or finish.
        /// </summary>
        protected abstract void OnArrived();

        /// <summary>
        ///     What altitude should we fly/stand at?
        /// </summary>
        protected abstract float GetMaintainY();

        private void UpdateHPBar(float currentHP, float maxHP)
        {
            if (FlaggedForRecycle) return;
            float progress = Mathf.Clamp01(currentHP / maxHP);
            // Calculate target width based on normalized HP
            float targetWidth = MaxHPBarWidth * progress;
            // Get the current size of the HP bar
            Vector2 currentSize = HpBarRect.sizeDelta;
            if (Mathf.Approximately(currentSize.x, targetWidth)) return;
            // Animate the size change using PrimeTween (duration is set to 0.5 seconds, adjust as needed)
            Tween.UISizeDelta(HpBarRect, new Vector2(targetWidth, currentSize.y), 0.2f, Ease.OutSine);
        }
    }
}