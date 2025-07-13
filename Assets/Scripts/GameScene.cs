using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TowerDefense.Controller;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.Pooling;
using TowerDefense.UI;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense
{
    public class GameScene : BaseScene, IBindingContext
    {
        private const int _max = 8;
        public GridManager GridManager;
        public GridInputHandler GridInputHandler;
        public TowerPrefabGenerator TowerPrefabGenerator;

        [HideInInspector] public Enums.GameState GameState = Enums.GameState.Start;

        public RectTransform TowerButtonPrefab;
        public Transform TowerButtonPrefabContainer;

        private readonly PlayerController _playerController = new();
        private int _current = 1;
        private List<EnemyModel> _enemyModels;
        private int _lastIndex = -1;

        public override void Awake()
        {
            base.Awake();
            GameManager.Instance.CurrentScene = this;
            _playerController.Initialize(new PlayerModel(50000, 0, 8));
            RegisterBindingContext();
            EventManager.OnGameStateChanged += GameStateChanged;
        }

        public override void Start()
        {
            base.Start();

            SetBindingData();
            GetTowerList();
            SetTowerButtonUI();
            EventManager.GameStateChanged(Enums.GameState.Start);
            GridManager.BuildGrid(32, 32);
        }

        public override void OnDestroy()
        {
            _playerController.OnSoftDestroy();
            EventManager.OnGameStateChanged -= GameStateChanged;
            UnregisterBindingContext();
        }

        void GameStateChanged(Enums.GameState newState)
        {
            switch (newState)
            {
                case Enums.GameState.Start:
                    break;
                case Enums.GameState.Preparing:
                    _lastIndex = -1;
                    break;
                case Enums.GameState.Playing:
                    EventManager.PlayerDidSomething(Enums.PlayerActions.NewLevel);
                    break;
                case Enums.GameState.GameOver:
                    EventManager.PlayerDidSomething(Enums.PlayerActions.GameOver);
                    break;
            }

            GameState = newState;
            GameStateText.Value = GameState.ToString();
            GameStateIndex.Value = (int)newState;
        }

        public void StartGame()
        {
            EventManager.GameStateChanged(Enums.GameState.Preparing);
            GetEnemyList();
        }

        public void RestartGame()
        {
            _playerController.Restart();
            EventManager.GameStateChanged(Enums.GameState.Start);
        }
        public void InitiateRound()
        {
            EventManager.GameStateChanged(Enums.GameState.Playing);
        }
        /// <summary>
        ///     API MOCKUP CALL
        /// </summary>
        void GetTowerList()
        {
            List<TowerModel> models = GameManager.Instance.Api.GetBuildingTypes();
            TowerPrefabGenerator.GenerateTowerPrefabs(models);
            TowerModels.Value = models;
        }

        void SetTowerButtonUI()
        {
            IList<TowerModel> models = TowerModels.Value;
            float x = 0f;
            const float spacing = 20f;

            for (int i = 0; i < models.Count; i++)
            {
                // instantiating directly under container (no world pos/rot)
                RectTransform btnRT = Instantiate(TowerButtonPrefab, TowerButtonPrefabContainer, false);
                btnRT.localScale = Vector3.one;

                // force left‐center anchoring & pivot
                btnRT.anchorMin = new Vector2(0f, 0.5f);
                btnRT.anchorMax = new Vector2(0f, 0.5f);
                btnRT.pivot = new Vector2(0f, 0.5f);

                // position relative to left edge of container
                btnRT.anchoredPosition = new Vector2(x, 0f);

                // initialize your button logic
                if (btnRT.TryGetComponent<TowerButton>(out TowerButton tb)) tb.Init(models[i], i);
                else LoggerExtra.LogError("TowerButton component missing!");

                btnRT.gameObject.SetActive(true);

                // advance x by width + spacing
                x += btnRT.rect.width + spacing;
            }
        }

        /// <summary>
        ///     API MOCKUP CALL
        /// </summary>
        public void GetEnemyList()
        {
            _enemyModels = GameManager.Instance.Api.GetEnemyTypes();
            EnemyPool.InitializePools(_enemyModels, GridManager.CurrentPathPositions);
        }

        /// <summary>
        ///     Handles plane selection in the carousel with visual feedback.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SelectTower(int index)
        {
            if (_lastIndex == index) return;
            _lastIndex = index;
            EventManager.GameStateChanged(Enums.GameState.Editing);
            TowerController tempTower = TowerPrefabGenerator.Towers[index];
            GridInputHandler.StartHover(tempTower.gameObject, tempTower.Model);
        }

        public bool CanIBuyThatTower(int value)
        {
            return _playerController.Model.Gold.Value >= value;
        }

        public void ChangeSpeed()
        {
            _current <<= 1;
            if (_current > _max) _current = 1;
            EventManager.TimeMultiplierEventNeeded(_current);
            Speed.Value = _current;
        }

        #region PoolRerefences

        public EnemyPool EnemyPool;
        public BulletPool BulletPool;

        #endregion

        #region Bindings

        public Bindable<string> GameStateText { get; private set; }
        public Bindable<int> GameStateIndex { get; } = new();
        public Bindable<int> Speed { get; } = new();
        public ListBinding<TowerModel> TowerModels { get; private set; }

        #endregion

        #region BindingContextInterface

        public void SetBindingData()
        {
            GameStateText = new Bindable<string>(GameState.ToString());
            Speed.Value = TimeManager.TimeScale.ToInt();
            TowerModels = new ListBinding<TowerModel>();
        }

        public void RegisterBindingContext()
        {
            BindingContextRegistry.Register(GetType().Name, this);
        }

        public void UnregisterBindingContext()
        {
            BindingContextRegistry.Unregister(GetType().Name, this);
        }

        #endregion
    }
}