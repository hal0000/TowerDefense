using System.Collections.Generic;
using TowerDefense.Controller;
using TowerDefense.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Core
{
    public class GridInputHandler : MonoBehaviour
    {
        private GridManager _gridManager;
        private GameObject _ghostPrefab;
        private TowerModel _towerModel;
        // simple 2×2 test footprint; replace with your dynamic footprint
        private static readonly string[] _debugFootprint = { "11", "11" };

        private Camera _cam;
        private Plane _groundPlane;
        private GameObject _ghostInstance;
        private int _originPacked;
        private List<int> _lastHighlighted = new List<int>();
        private bool _startHover = false;
        void Awake()
        {
            _cam = Camera.main;
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (GameManager.Instance.CurrentScene is GameScene gs)
                _gridManager = gs.GridManager;
        }

         void Update()
        {
            if (!_startHover) return;
            HandleGhostDrag();
        }
        public void StartHover(GameObject prefab, TowerModel model)
        {
            
            _ghostPrefab = prefab;
            _towerModel = model;
            if (_ghostInstance != null) Destroy(_ghostInstance);
            _startHover = true;
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
            _ghostInstance.GetComponent<TowerController>().Initialize(_towerModel);
            _ghostInstance.SetActive(true);
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
                var go = Instantiate(_ghostInstance,_ghostInstance.transform.position,_ghostInstance.transform.rotation);
                go.GetComponent<TowerController>().Initialize(_towerModel);

                // 3) Clean up the ghost
                Destroy(_ghostInstance);
                _ghostInstance = null;
                _startHover = false;
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