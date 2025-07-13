using System.Collections.Generic;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class GroundEnemyController : BaseEnemyController
    {
        public override void Initialize(EnemyModel model, IReadOnlyList<Vector3> waypoints)
        {
            base.Initialize(model, waypoints);
            WaypointIndex = 0;
        }

        protected override bool GetCurrentTarget(out Vector3 target)
        {
            if (WaypointIndex >= Waypoints.Count)
            {
                target = default;
                return false;
            }

            target = Waypoints[WaypointIndex];
            return true;
        }

        protected override void OnArrived()
        {
            WaypointIndex++;
        }

        protected override float GetMaintainY()
        {
            return 0f;
        }
    }
}