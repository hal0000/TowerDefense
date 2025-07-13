using System.Collections.Generic;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class FlyingEnemyController : BaseEnemyController
    {
        [SerializeField] private float FlightHeight = 2f;
        private int _totalWayCounts;

        public override void Initialize(EnemyModel model, IReadOnlyList<Vector3> waypoints)
        {
            base.Initialize(model, waypoints);
            _totalWayCounts = waypoints.Count - 1;
            WaypointIndex = _totalWayCounts;
        }

        protected override bool GetCurrentTarget(out Vector3 target)
        {
            if (WaypointIndex < 0)
            {
                target = default;
                return false;
            }

            target = Waypoints[WaypointIndex];
            return true;
        }

        protected override void OnArrived()
        {
            WaypointIndex = -1;
        }

        protected override float GetMaintainY()
        {
            return FlightHeight;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            WaypointIndex = _totalWayCounts;
        }
    }
}