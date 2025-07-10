using System;
using System.Collections;
using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Controller;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.Pooling;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense
{
    public class GameScene : BaseScene, IBindingContext
    {
        public GridManager GridManager; 
        public GridInputHandler GridInputHandler;
        public EnemyPool EnemyPool;
        public Bindable<string> GameStateText { get; private set; }
        public Bindable<int> GameStateIndex { get; } = new();
        [HideInInspector] public Enums.GameState GameState = Enums.GameState.Nothing;
        public List<TowerController> Towers = new List<TowerController>();
        public List<EnemyController> Enemys;

        public TowerController SelectedTower;
        private List<EnemyModel> _enemyModels;

        public override void Awake()
        {
            base.Awake();
            GameManager.Instance.CurrentScene = this;
            RegisterBindingContext();
        }

        public override void Start()
        {
            base.Start();
            GameStateChanged(Enums.GameState.Nothing);
            SetBindingData();
            GetTowerList();
        }

        public void GameStateChanged(Enums.GameState newState)
        {
            switch (newState)
            {
                case Enums.GameState.Nothing:
                    break;
                case Enums.GameState.Preparing:
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
            foreach (var model in models)
            {
                int idx = model.Index;
                if (idx < 0 || idx >= Towers.Count)
                {
                    LoggerExtra.LogWarning($"GameScene.GetTowerList(): no prefab assigned at index {idx}");
                    continue;
                }
                Towers[idx].Initialize(model);
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
        
        public void SelectPrefab(int index)
        {
            SelectedTower = Towers[index];
            GridInputHandler.StartHover(SelectedTower.gameObject, SelectedTower.Model);
        }

#region BindingContextInterface

        public void SetBindingData()
        {
            GameStateText = new Bindable<string>(GameState.ToString());
        }
        public void RegisterBindingContext() => BindingContextRegistry.Register(GetType().Name, this);
        public void UnregisterBindingContext() => BindingContextRegistry.Unregister(GetType().Name, this);
#endregion
    }
}