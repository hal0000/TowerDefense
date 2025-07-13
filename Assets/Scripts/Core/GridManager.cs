using System.Collections.Generic;
using TowerDefense.Controller;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Core
{
    [RequireComponent(typeof(Transform))]
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")] [Tooltip("Prefab that contains a CellView component")] [SerializeField]
        private CellController CellPrefab;

        private CellController[] _cells;

        /// <summary>
        /// </summary>
        private Vector3[] _currentPathPositions;

        public IReadOnlyList<Vector3> CurrentPathPositions => _currentPathPositions;
        public int Width { get; private set; }

        public int Height { get; private set; }

        /// <summary>
        ///     A simple random‐axis walk from (0,0) → (w-1,h-1).
        /// </summary>
        private List<int> ComputePath(int w, int h)
        {
            List<int> fullPath = new();
            List<int> corners = new();

            int x = 0, y = 0, tx = w - 1, ty = h - 1;
            fullPath.Add(CoordPacker.Pack(x, y));
            corners.Add(fullPath[0]);

            int prevX = x, prevY = y;
            Vector2Int lastDir = Vector2Int.zero;

            while (x != tx || y != ty)
            {
                bool canX = x != tx, canY = y != ty;
                if (canX && (!canY || Random.value < 0.5f)) x += tx > x ? 1 : -1;
                else y += ty > y ? 1 : -1;
                int packed = CoordPacker.Pack(x, y);
                fullPath.Add(packed);

                // detecting direction change for finding curve/corner
                Vector2Int dir = new(x - prevX, y - prevY);
                if (lastDir != Vector2Int.zero && dir != lastDir)
                    // record the _previous_ cell as a corner
                    corners.Add(CoordPacker.Pack(prevX, prevY));

                lastDir = dir;
                prevX = x;
                prevY = y;
            }

            // lastly add the final cell too
            corners.Add(fullPath[^1]);

            // convert corners → world‐space and stash
            _currentPathPositions = new Vector3[corners.Count];
            for (int i = 0; i < corners.Count; i++)
            {
                _currentPathPositions[i] = GetCellCenter(corners[i]);
            }

            return fullPath;
        }


        public Vector3[] BuildGrid(int w, int h)
        {
            // if we already built once just reset fields
            if (_cells != null && _cells.Length > 0)
            {
                ResetGrid();
                return _currentPathPositions;
            }

            Width = w;
            Height = h;
            _cells = new CellController[w * h];

            HashSet<int> pathSet = new(ComputePath(w, h));
            Quaternion rot = CellPrefab.gameObject.transform.rotation;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int packed = CoordPacker.Pack(x, y);
                    int index = y * w + x; // flat‐array index

                    CellController go = Instantiate(CellPrefab, new Vector3(x, 0f, y), rot, transform);
                    go.gameObject.SetActive(true);
#if UNITY_EDITOR
                    go.name = $"Cell_{x}_{y}";
#endif
                    CellModel cm = new(go.name, x, y);
                    if (pathSet.Contains(packed)) cm.SetPath(true);
                    go.Initialize(cm);
                    _cells[index] = go;
                }
            }

            return _currentPathPositions;
        }

        private void ResetGrid()
        {
            for (int i = 0, len = _cells.Length; i < len; i++)
            {
                CellController cv = _cells[i];
                cv.Model.SetOccupied(false);
                cv.Model.SetPath(false);
                cv.ResetHighlight();
            }
        }

        private bool TryGetIndex(int x, int y, out int index)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                index = -1;
                return false;
            }

            index = y * Width + x;
            return true;
        }

        public bool WorldToPackedCoord(Vector3 worldPos, out int packed)
        {
            int x = Mathf.FloorToInt(worldPos.x);
            int y = Mathf.FloorToInt(worldPos.z);
            packed = CoordPacker.Pack(x, y);
            return TryGetIndex(x, y, out _);
        }

        public CellController GetCellView(int packed)
        {
            int x = CoordPacker.UnpackX(packed);
            int y = CoordPacker.UnpackY(packed);
            return TryGetIndex(x, y, out int idx) ? _cells[idx] : null;
        }

        public Vector3 GetCellCenter(int packed)
        {
            int x = CoordPacker.UnpackX(packed), y = CoordPacker.UnpackY(packed);
            return new Vector3(x + .5f, 0f, y + .5f);
        }
    }
}