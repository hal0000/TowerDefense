using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Core
{
    public class GridInputHandler : MonoBehaviourExtra
    {
        [Header("References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private GameObject _ghostPrefab;

        // simple 2×2 test footprint; replace with your dynamic footprint
        private static readonly string[] _debugFootprint = { "11", "11" };

        private Camera _cam;
        private Plane _groundPlane;
        private GameObject _ghostInstance;
        private int _originPacked;
        private List<int> _lastHighlighted = new List<int>();

        void Awake()
        {
            _cam = Camera.main;
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        protected override void Tick()
        {
            HandleGhostDrag();
        }

        private void HandleGhostDrag()
        {
            // 1) read mouse
            if (Mouse.current == null) return;
            Vector2 mp = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mp);

            // 2) project into y=0 plane
            if (!_groundPlane.Raycast(ray, out float enter))
            {
                ClearGhost();
                return;
            }
            Vector3 worldHit = ray.GetPoint(enter);

            // 3) world→grid packed‐int
            if (!_gridManager.WorldToPackedCoord(worldHit, out int packed))
            {
                ClearGhost();
                return;
            }

            // 4) if ghost not spawned or moved, (re)create it
            if (_ghostInstance == null || packed != _originPacked)
            {
                _originPacked = packed;
                ClearGhost(); // clear any old highlights
                SpawnGhostAt(packed);
            }

            // 5) gather footprint cells
            var footprintCells = new List<int>();
            int ox = CoordPacker.UnpackX(packed),
                oy = CoordPacker.UnpackY(packed);

            for (int row = 0; row < _debugFootprint.Length; row++)
            {
                string line = _debugFootprint[row];
                for (int col = 0; col < line.Length; col++)
                {
                    if (line[col] == '1')
                        footprintCells.Add(CoordPacker.Pack(ox + col, oy + row));
                }
            }

            // 6) test validity
            bool valid = true;
            foreach (int p in footprintCells)
            {
                var cv = _gridManager.GetCellView(p);
                if (cv == null || cv.Model.IsOccupied || cv.Model.IsPath)
                {
                    valid = false;
                    break;
                }
            }

            // 7) highlight footprint
            Color tint = valid ? Color.green : Color.red;
            foreach (int p in footprintCells)
            {
                var cv = _gridManager.GetCellView(p);
                if (cv != null)
                    cv.Highlight(valid);
            }
            _lastHighlighted.AddRange(footprintCells);

            // 8) on mouse-up, commit or clear
            if (Mouse.current.leftButton.wasReleasedThisFrame)
                CommitOrClear(valid, footprintCells);
        }

        private void SpawnGhostAt(int packed)
        {
            Vector3 center = _gridManager.GetCellCenter(packed);
            Quaternion rot = _ghostPrefab.transform.rotation;
            _ghostInstance = Instantiate(_ghostPrefab, center,rot);
        }

        private void CommitOrClear(bool valid, List<int> footprintCells)
        {
            if (valid && _ghostInstance != null)
            {
                // 1) Mark every footprint cell as occupied
                foreach (int p in footprintCells)
                {
                    var cv = _gridManager.GetCellView(p);
                    if (cv != null)
                        cv.Model.SetOccupied(true);
                }

                // 2) Instantiate exactly one building at the ghost’s position
                Instantiate(_ghostInstance,_ghostInstance.transform.position,_ghostInstance.transform.rotation);

                // 3) Clean up the ghost
                Destroy(_ghostInstance);
                _ghostInstance = null;
            }
            else
            {
                Destroy(_ghostInstance);
                _ghostInstance = null;
            }

            ClearGhostHighlights();
        }

        private void ClearGhost()
        {
            if (_ghostInstance != null)
            {
                Destroy(_ghostInstance);
                _ghostInstance = null;
            }
            ClearGhostHighlights();
        }

        private void ClearGhostHighlights()
        {
            foreach (int p in _lastHighlighted)
            {
                var cv = _gridManager.GetCellView(p);
                if (cv != null) cv.ResetHighlight();
            }
            _lastHighlighted.Clear();
        }
    }
}