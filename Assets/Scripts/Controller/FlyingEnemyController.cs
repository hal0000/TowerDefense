using System.Collections.Generic;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class FlyingEnemyController : BaseEnemyController
    {
        [SerializeField] private float FlightHeight = 2f;

        public override void Initialize(EnemyModel model, IReadOnlyList<Vector3> waypoints)
        {
            base.Initialize(model, waypoints);
            WaypointIndex = waypoints.Count - 1;
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

        protected override float GetMaintainY() => FlightHeight;
    }
}