using System.Collections.Generic;
using TowerDefense.Controller;
using TowerDefense.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Core
{
    public class GridInputHandler : MonoBehaviour
    {
        GridManager _gridManager;
        GameObject _ghostPrefab;
        TowerModel _towerModel;
        GameScene _scene;
        Camera _cam;
        Plane _groundPlane;
        GameObject _ghostInstance;
        int _originPacked;
        readonly List<int> _lastHighlighted = new List<int>();
        readonly List<int> _footprintCells = new List<int>();
        bool _startHover;

        void Awake()
        {
            _cam = Camera.main;
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (GameManager.Instance.CurrentScene is not GameScene gs) return;
            _scene = gs;
            _gridManager = gs.GridManager;
        }

        void Update()
        {
            if (_startHover)
                HandleGhostDrag();
        }

        public void StartHover(GameObject prefab, TowerModel model)
        {
            _ghostPrefab = prefab;
            _towerModel = model;
            if (_ghostInstance != null) DestroyImmediate(_ghostInstance.gameObject);
            _lastHighlighted.Clear();
            _startHover = true;
        }

        void HandleGhostDrag()
        {
            if (Mouse.current == null) return;
            var mp = Mouse.current.position.ReadValue();
            var ray = _cam.ScreenPointToRay(mp);
            if (!_groundPlane.Raycast(ray, out float enter)) { ClearGhost(); return; }

            Vector3 worldHit = ray.GetPoint(enter);
            if (!_gridManager.WorldToPackedCoord(worldHit, out int packed)) { ClearGhost(); return; }

            var center = _gridManager.GetCellCenter(packed);

            if (_ghostInstance == null)
            {
                _ghostInstance = Instantiate(_ghostPrefab, center, _ghostPrefab.transform.rotation);
                if (_ghostInstance.TryGetComponent<TowerController>(out var temp)) temp.Initialize(_towerModel);
                _ghostInstance.SetActive(true);
            }
            else
            {
                _ghostInstance.transform.SetPositionAndRotation(center, _ghostPrefab.transform.rotation); 
            }

            _originPacked = packed;
            ClearGhostHighlights();

            // build footprint from TowerModel.byte[] grid
            _footprintCells.Clear();
            int rows = _towerModel.Rows, cols = _towerModel.Cols;
            int ox = CoordPacker.UnpackX(packed),
                oy = CoordPacker.UnpackY(packed);

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (_towerModel.GetCell(r, c) == 1)
                        _footprintCells.Add(CoordPacker.Pack(ox + c, oy + r));

            bool valid = true;
            foreach (int p in _footprintCells)
            {
                var cv = _gridManager.GetCellView(p);
                if (cv == null || cv.Model.IsOccupied || cv.Model.IsPath)
                {
                    valid = false;
                    break;
                }
            }

            foreach (int p in _footprintCells)
            {
                var cv = _gridManager.GetCellView(p);
                if (cv != null)
                    cv.Highlight(valid);
                _lastHighlighted.Add(p);
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                CommitOrClear(valid);
        }

        void CommitOrClear(bool valid)
        {
            if (valid && _ghostInstance != null)
            {
                foreach (int p in _footprintCells)
                {
                    var cv = _gridManager.GetCellView(p);
                    if (cv != null)
                        cv.Model.SetOccupied(true);
                }
                var go = Instantiate(_ghostInstance, _ghostInstance.transform.position, _ghostInstance.transform.rotation);
                
                go.GetComponent<TowerController>().Initialize(_towerModel);
                _scene.TowerPlaced();
                _ghostInstance.SetActive(false);
                _startHover = false;
            }
            else if (_ghostInstance != null)
            {
                DestroyImmediate(_ghostInstance.gameObject);
            }

            ClearGhostHighlights();
        }

        void ClearGhost()
        {
            if (_ghostInstance != null) DestroyImmediate(_ghostInstance.gameObject);
            ClearGhostHighlights();
        }

        void ClearGhostHighlights()
        {
            foreach (int p in _lastHighlighted)
            {
                var cv = _gridManager.GetCellView(p);
                if (cv != null)
                    cv.ResetHighlight();
            }
            _lastHighlighted.Clear();
        }
    }
}