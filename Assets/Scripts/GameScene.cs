using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TowerDefense.Core;
using TowerDefense.Controller;
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
        public GridManager GridManager; 
        public GridInputHandler GridInputHandler;
        public TowerPrefabGenerator TowerPrefabGenerator;
#region PoolRerefences
        public EnemyPool EnemyPool;
        public BulletPool BulletPool;
#endregion

#region Bindings
        public Bindable<string> GameStateText { get; private set; }
        public Bindable<int> GameStateIndex { get; } = new();
        public ListBinding<TowerModel> TowerModels { get; private set; }
#endregion



        [HideInInspector] public Enums.GameState GameState = Enums.GameState.Nothing;
        //public List<TowerController> Towers = new List<TowerController>();
        public List<EnemyController> Enemys;
        public RectTransform TowerButtonPrefab;
        public Transform TowerButtonPrefabContainer;


        private List<EnemyModel> _enemyModels;
        private int _lastIndex = -1;
        public override void Awake()
        {
            base.Awake();
            GameManager.Instance.CurrentScene = this;
            RegisterBindingContext();
        }

        public override void Start()
        {
            base.Start();
            SetBindingData();
            GetTowerList();
            SetTowerButtonUI();
            GameStateChanged(Enums.GameState.Nothing);
        }

        public void GameStateChanged(Enums.GameState newState)
        {
            switch (newState)
            {
                case Enums.GameState.Nothing:
                    break;
                case Enums.GameState.Preparing:
                    _lastIndex = -1;
                    break;
                case Enums.GameState.Playing:
                    break;
                case Enums.GameState.Endgame:
                    break;
            
            }
            GameState = newState;
            GameStateText.Value = GameState.ToString();
            GameStateIndex.Value = (int)newState;
        }
        public void StartGame()
        {
            GridManager.BuildGrid(30, 30);
            GameStateChanged(Enums.GameState.Preparing);
            GetEnemyList();
        }

        public void InitiateRound()
        {
            GameStateChanged(Enums.GameState.Playing);
            StartCoroutine(SpawnWave(10, 5f));
        }

        /// <summary>
        /// API MOCKUP CALL
        /// </summary>
        public void GetTowerList()
        {
            List<TowerModel> models = GameManager.Instance.Api.GetBuildingTypes();
            TowerPrefabGenerator.GenerateTowerPrefabs(models);
            TowerModels.Value = models;
            // ///PREFAB DATA IMPLEMENTATION
            // var tempList = TowerPrefabGenerator.Towers;
            // var counter = tempList.Count;
            // foreach (var model in models)
            // {
            // int idx = model.Index;
            // if (idx < 0 || idx >= counter)
            // {
            // LoggerExtra.LogWarning($"GameScene.GetTowerList(): no prefab assigned at index {idx}");
            // continue;
            // }
            // tempList[idx].Initialize(model);
            // }
        }

        public void SetTowerButtonUI()
        {
            var models = TowerModels.Value;
            float x = 0f;
            const float spacing = 20f;

            for (int i = 0; i < models.Count; i++)
            {
                // instantiating directly under container (no world pos/rot)
                var btnRT = Instantiate(TowerButtonPrefab, TowerButtonPrefabContainer, false);
                btnRT.localScale = Vector3.one;

                // force left‐center anchoring & pivot
                btnRT.anchorMin = new Vector2(0f, 0.5f);
                btnRT.anchorMax = new Vector2(0f, 0.5f);
                btnRT.pivot = new Vector2(0f, 0.5f);

                // position relative to left edge of container
                btnRT.anchoredPosition = new Vector2(x, 0f);

                // initialize your button logic
                if (btnRT.TryGetComponent<TowerButton>(out var tb)) tb.Init(models[i], i);
                else LoggerExtra.LogError("TowerButton component missing!");

                btnRT.gameObject.SetActive(true);

                // advance x by width + spacing
                x += btnRT.rect.width + spacing;
            }
        }
        /// <summary>
        /// API MOCKUP CALL
        /// </summary>
        public void GetEnemyList()
        {
            _enemyModels = GameManager.Instance.Api.GetEnemyTypes();
            EnemyPool.InitializePools(_enemyModels,GridManager.CurrentPathPositions);
        }

        /// <summary>
        /// Spawning Wave
        /// </summary>
        private IEnumerator SpawnWave(int totalEnemies, float perSecond)
        {
            float interval = 1f / perSecond;
            for (int i = 0; i < totalEnemies; i++)
            {
                EnemyModel model = _enemyModels[0];
                EnemyPool.GetEnemy(model.Type);
                yield return new WaitForSeconds(interval);
            }
        }
        /// <summary>
        /// Handles plane selection in the carousel with visual feedback.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SelectTower(int index)
        {
            if (_lastIndex == index) return;
            _lastIndex = index;
            
            var tempTower = TowerPrefabGenerator.Towers[index];
            GridInputHandler.StartHover(tempTower.gameObject, tempTower.Model);
        }

        public void TowerPlaced()
        {
            _lastIndex = -1;
        }
        #region BindingContextInterface

        public void SetBindingData()
        {
            GameStateText = new Bindable<string>(GameState.ToString());
            TowerModels = new ListBinding<TowerModel>();

        }
        public void RegisterBindingContext() => BindingContextRegistry.Register(GetType().Name, this);
        public void UnregisterBindingContext() => BindingContextRegistry.Unregister(GetType().Name, this);
#endregion
    }
}