using System.Collections;
using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Model;
using TowerDefense.Pooling;
using TowerDefense.ScriptableObject;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class WaveSpawnController : MonoBehaviour
    {
        [Header("Wave Settings")] [SerializeField]
        private WaveConfig _waveConfig;

        private int _activeEnemyCount;
        private int _currentWaveIndex;
        private EnemyPool _enemyPool;

        private Enums.GameState _lastState;

        private void Awake()
        {
            EventManager.OnGameStateChanged += OnGameStateChanged;
            EventManager.OnPlayerAction += PlayerACtionHandlerForSpawn;
            if (GameManager.Instance.CurrentScene is GameScene scene) _enemyPool = scene.EnemyPool;
        }

        private void OnDestroy()
        {
            EventManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void PlayerACtionHandlerForSpawn(Enums.PlayerActions type, int value)
        {
            switch (type)
            {
                case Enums.PlayerActions.GetDamage:
                case Enums.PlayerActions.EnemyKilled:
                    _activeEnemyCount--;
                    break;
            }

            if (_activeEnemyCount == 0 && _lastState == Enums.GameState.Playing) EventManager.GameStateChanged(Enums.GameState.Preparing);
        }

        private void OnGameStateChanged(Enums.GameState state)
        {
            _lastState = state;
            switch (state)
            {
                case Enums.GameState.Playing:
                    StartNextWave();
                    break;
                case Enums.GameState.Start:
                    _currentWaveIndex = 0;
                    break;
            }
        }

        private void StartNextWave()
        {
            if (_currentWaveIndex >= _waveConfig.Waves.Count)
            {
                EventManager.GameStateChanged(Enums.GameState.EndGame);
                return;
            }

            WaveModel wave = _waveConfig.Waves[_currentWaveIndex];
            StartCoroutine(SpawnWaveRoutine(wave));
            _currentWaveIndex++;
        }

        private IEnumerator SpawnWaveRoutine(WaveModel wave)
        {
            float interval = 1f / wave.SpawnRatePerSecond;
            List<SpawnModel> tempList = wave.Enemy;
            int count = tempList.Count;
            _activeEnemyCount = 0;
            foreach (SpawnModel spawn in wave.Enemy)
                _activeEnemyCount += spawn.Count;
            foreach (SpawnModel spawn in wave.Enemy)
                for (int i = 0; i < spawn.Count; i++)
                {
                    _enemyPool.GetEnemy(spawn.EnemyType);

                    float elapsed = 0f;
                    while (elapsed < interval)
                    {
                        elapsed += TimeManager.DeltaTime;
                        yield return null;
                    }
                }
        }
    }
}