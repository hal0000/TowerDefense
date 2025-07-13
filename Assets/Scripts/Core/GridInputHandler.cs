using System.Collections;
using System.Collections.Generic;
using TowerDefense.Controller;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.UI.Binding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TowerDefense.Core
{
    public class GridInputHandler : MonoBehaviour, IBindingContext
    {
        [Header("Ground Raycast")] [SerializeField]
        private LayerMask _groundLayer;

        [SerializeField] private LayerMask _towerLayer;
        private readonly List<int> _footprintCells = new();
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[1];
        private readonly List<int> _lastHighlighted = new();
        private Camera _cam;
        private bool _canCommit;
        private bool _canEditTower;
        private GameObject _ghostInstance;
        private GameObject _ghostPrefab;

        private GridManager _gridManager;
        private bool _isEditing;
        private TowerController _pendingTower;
        private TowerController _pickedTower;
        private GameScene _scene;
        private bool _startHover;
        private TowerModel _towerModel;

        public Bindable<int> TowerState { get; } = new();
        public Bindable<int> TowerGold { get; } = new();
        public Bindable<int> TowerUpgrade { get; } = new();

        public bool CanIMoveCamera { get; private set; }

        private void Awake()
        {
            RegisterBindingContext();
            _cam = Camera.main;
            if (GameManager.Instance.CurrentScene is GameScene gs)
            {
                _scene = gs;
                _gridManager = gs.GridManager;
            }

            EventManager.OnGameStateChanged += GameStateChanged;
        }

        private void Update()
        {
            if (!_canEditTower) return;

            // read all actual touches (EnhancedTouch)
            ReadOnlyArray<Touch> touches = Touch.activeTouches;
            if (touches.Count == 0) return;

            // take the first finger
            Touch t0 = touches[0];
            Vector2 screenPos = t0.screenPosition;

            // ignore if over any UI
            if (EventSystem.current.IsPointerOverGameObject(t0.touchId)) return;
            // build a ray from camera touch point
            Ray ray = _cam.ScreenPointToRay(screenPos);
            if (!CanIMoveCamera)
            {
                int towerHits = Physics.RaycastNonAlloc(ray, _hitBuffer, Mathf.Infinity, _towerLayer);
                if (towerHits > 0)
                    if (_hitBuffer[0].collider.transform.parent.gameObject.TryGetComponent(out TowerController temp))
                        if (temp.CanIEdit)
                        {
                            _pendingTower = temp;
                            StartCoroutine(BeginMoveDelayed(temp));
                            EventManager.GameStateChanged(Enums.GameState.Editing);
                            return;
                        }
            }

            if (!_startHover) return;
            int groundHits = Physics.RaycastNonAlloc(ray, _hitBuffer, Mathf.Infinity, _groundLayer);
            if (groundHits == 0) return;
            Vector3 hitPoint = _hitBuffer[0].point;
            if (!_gridManager.WorldToPackedCoord(hitPoint, out int packed)) return;
            CanIMoveCamera = true;
            HandleGhostDrag(packed);
        }

        private void OnDestroy()
        {
            EventManager.OnGameStateChanged -= GameStateChanged;
            UnregisterBindingContext();
        }

        public void SetBindingData()
        {
            //BindingContextRegistry.Register(GetType().Name, this);
        }

        public void RegisterBindingContext()
        {
            BindingContextRegistry.Register(GetType().Name, this);
        }

        public void UnregisterBindingContext()
        {
            BindingContextRegistry.Unregister(GetType().Name, this);
        }

        private void GameStateChanged(Enums.GameState type)
        {
            _canEditTower = type == Enums.GameState.Editing || type == Enums.GameState.Preparing;
            if (type == Enums.GameState.Start)
            {
                int count = transform.childCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }

        private void BeginMove(TowerController tower)
        {
            _pickedTower = tower;
            tower.gameObject.SetActive(false);
            foreach (int cell in tower.OccupiedCells)
                _gridManager.GetCellView(cell).Model.SetOccupied(false);
            StartHover(tower.gameObject, tower.Model, true);
        }

        public void StartHover(GameObject prefab, TowerModel model, bool isEdit = false)
        {
            if (CanIMoveCamera) return;
            if (isEdit)
            {
                _isEditing = true;
                _canCommit = true;
                _ghostPrefab = prefab;
                _towerModel = model;
                if (_ghostInstance != null) DestroyImmediate(_ghostInstance.gameObject);
                _ghostInstance = Instantiate(_ghostPrefab, prefab.transform.position, prefab.transform.rotation, transform);
                if (_ghostInstance.TryGetComponent(out TowerController temp))
                {
                    temp.Initialize(_towerModel);
                    _ghostInstance.SetActive(true);
                }

                TowerGold.Value = model.Gold.ToInt();
                TowerUpgrade.Value = model.Gold.ToInt() * model.Level;
                TowerState.Value = (int)Enums.TowerOptions.OldTower;
            }
            else
            {
                _isEditing = false;
                _ghostPrefab = prefab;
                _towerModel = model;
                if (_ghostInstance != null)
                    DestroyImmediate(_ghostInstance.gameObject);

                ClearGhostHighlights();

                _canCommit = false;
                int centerX = _gridManager.Width / 2;
                int centerY = _gridManager.Height / 2;
                int centerPacked = CoordPacker.Pack(centerX, centerY);

                Vector3 centerPos = _gridManager.GetCellCenter(centerPacked);
                _ghostInstance = Instantiate(_ghostPrefab, centerPos, _ghostPrefab.transform.rotation, transform);
                if (_ghostInstance.TryGetComponent(out TowerController temp))
                {
                    temp.Initialize(_towerModel);
                }

                _ghostInstance.GetComponent<TowerController>().Initialize(_towerModel);
                _footprintCells.Clear();
                int rows = _towerModel.Rows, cols = _towerModel.Cols;
                int ox = centerX, oy = centerY;
                for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    if (_towerModel.GetCell(r, c) == 1)
                        _footprintCells.Add(CoordPacker.Pack(ox + c, oy + r));
                }

                bool valid = true;
                foreach (int p in _footprintCells)
                {
                    CellController cv = _gridManager.GetCellView(p);
                    if (cv != null && !cv.Model.IsOccupied && !cv.Model.IsPath) continue;
                    valid = false;
                    break;
                }

                foreach (int p in _footprintCells)
                {
                    CellController cv = _gridManager.GetCellView(p);
                    if (cv != null) cv.Highlight(valid);
                    _lastHighlighted.Add(p);
                }

                _ghostInstance.SetActive(true);
                _canCommit = valid;
                TowerGold.Value = model.Gold.ToInt();
                TowerState.Value = (int)Enums.TowerOptions.NewTower;
            }

            CanIMoveCamera = _startHover = true;
        }

        private void HandleGhostDrag(int packed)
        {
            Vector3 center = _gridManager.GetCellCenter(packed);
            _ghostInstance.transform.SetPositionAndRotation(center, _ghostPrefab.transform.rotation);
            ClearGhostHighlights();

            // build footprint
            _footprintCells.Clear();
            int rows = _towerModel.Rows, cols = _towerModel.Cols;
            int ox = CoordPacker.UnpackX(packed), oy = CoordPacker.UnpackY(packed);

            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (_towerModel.GetCell(r, c) == 1)
                    _footprintCells.Add(CoordPacker.Pack(ox + c, oy + r));
            }

            bool isValid = true;
            foreach (int p in _footprintCells)
            {
                CellController cv = _gridManager.GetCellView(p);
                if (cv != null && !cv.Model.IsOccupied && !cv.Model.IsPath) continue;
                isValid = false;
                break;
            }

            foreach (int p in _footprintCells)
            {
                CellController cv = _gridManager.GetCellView(p);
                if (cv != null) cv.Highlight(_canCommit);
                _lastHighlighted.Add(p);
            }

            _canCommit = isValid;
        }

        /// <summary>
        ///     Called from Tower Prefab
        /// </summary>
        public void PutTower()
        {
            if (!_canCommit)
            {
                EventManager.NewNotificationHappened(Enums.NotificationType.TowerPositionIsNotValid);
                return;
            }

            bool playerHaveGold = _scene.CanIBuyThatTower(_isEditing ? 0 : _towerModel.Gold);
            if (!playerHaveGold)
            {
                EventManager.NewNotificationHappened(Enums.NotificationType.NotEnoughGold);
                return;
            }

            if (!_isEditing)
                foreach (int p in _footprintCells)
                {
                    CellController cv = _gridManager.GetCellView(p);
                    if (cv != null) cv.Model.SetOccupied(true);
                }

            TowerState.Value = (int)Enums.TowerOptions.Nothing;
            GameObject go = Instantiate(_ghostInstance, _ghostInstance.transform.position, _ghostInstance.transform.rotation, transform);
            if (go.TryGetComponent(out TowerController temp))
            {
                temp.OccupiedCells.Clear();
                temp.OccupiedCells.AddRange(_footprintCells);
                temp.Initialize(_towerModel);
                temp.Bauen();
            }

            if (_isEditing) Destroy(_pickedTower.gameObject);
            EventManager.PlayerDidSomething(Enums.PlayerActions.SpendGold, _isEditing ? 0 : _towerModel.Gold);
            Destroy(_ghostInstance.gameObject);
            _isEditing = _canCommit = CanIMoveCamera = _startHover = false;
            ClearGhostHighlights();
            EventManager.GameStateChanged(Enums.GameState.Preparing);
        }

        /// <summary>
        ///     Called from Tower Prefab Cancel action
        /// </summary>
        public void ClearGhost()
        {
            if (_pickedTower != null)
            {
                List<int> temp = _pickedTower.OccupiedCells;
                foreach (int p in temp)
                {
                    CellController cv = _gridManager.GetCellView(p);
                    if (cv != null) cv.Model.SetOccupied(true);
                }

                _pickedTower.gameObject.SetActive(true);
                _canCommit = false;
                _pickedTower = null;
            }

            _isEditing = CanIMoveCamera = _startHover = false;
            if (_ghostInstance != null) DestroyImmediate(_ghostInstance.gameObject);
            ClearGhostHighlights();
            EventManager.GameStateChanged(Enums.GameState.Preparing);
            TowerState.Value = (int)Enums.TowerOptions.Nothing;
        }

        private void ClearGhostHighlights()
        {
            foreach (int p in _lastHighlighted)
            {
                CellController cv = _gridManager.GetCellView(p);
                if (cv != null)
                    cv.ResetHighlight();
            }

            _lastHighlighted.Clear();
        }

        public void DeleteTower()
        {
            if (_pickedTower != null)
            {
                List<int> temp = _pickedTower.OccupiedCells;
                foreach (int p in temp)
                {
                    CellController cv = _gridManager.GetCellView(p);
                    if (cv != null) cv.Model.SetOccupied(false);
                }

                Destroy(_pickedTower.gameObject);
                _canCommit = false;
                _pickedTower = null;
            }

            _isEditing = CanIMoveCamera = _startHover = false;
            if (_ghostInstance != null) DestroyImmediate(_ghostInstance.gameObject);
            ClearGhostHighlights();
            EventManager.GameStateChanged(Enums.GameState.Preparing);
            TowerState.Value = Enums.TowerOptions.Nothing.ToInt();
        }

        public void UpgradeTower()
        {
            bool playerHaveGold = _scene.CanIBuyThatTower(_towerModel.Gold * _towerModel.Level);
            if (!playerHaveGold)
            {
                EventManager.NewNotificationHappened(Enums.NotificationType.NotEnoughGold);
                return;
            }

            EventManager.PlayerDidSomething(Enums.PlayerActions.SpendGold, _towerModel.Gold * _towerModel.Level);
            _pickedTower.Upgrade();
            ClearGhost();
        }

        private IEnumerator BeginMoveDelayed(TowerController tower)
        {
            yield return new WaitForSeconds(0.15f);

            // if the user cancelled in the meantime, cancel out
            if (!_canEditTower || _pendingTower != tower)
            {
                _pendingTower = null;
                yield break;
            }

            // now do the real begin‐move
            BeginMove(tower);
            CanIMoveCamera = true;
            _pendingTower = null;
        }
    }
}