using System;
using System.Collections;
using TowerDefense.Core;
using TowerDefense.Model;
using TowerDefense.Pooling;
using TowerDefense.ScriptableObject;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class WaveSpawnController : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private WaveConfig _waveConfig;

        private int _activeEnemyCount;
        private int _currentWaveIndex;
        private EnemyPool _enemyPool;
        void Awake()
        {
            EventManager.OnGameStateChanged += OnGameStateChanged;
            EventManager.OnPlayerAction += PlayerACtionHandlerForSpawn;
            if (GameManager.Instance.CurrentScene is GameScene scene) _enemyPool = scene.EnemyPool;
        }

        void OnDestroy()
        {
            EventManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void PlayerACtionHandlerForSpawn(Enums.PlayerActions type, int value)
        {
            switch(type)
            {
                case Enums.PlayerActions.GetDamage:
                case Enums.PlayerActions.EnemyKilled:
                    _activeEnemyCount--;
                    break;
            }

            if (_activeEnemyCount == 0)
            {
                EventManager.GameStateChanged(Enums.GameState.Preparing);
            }
        }
        private void OnGameStateChanged(Enums.GameState state)
        {
            if (state == Enums.GameState.Playing)
            {
                StartNextWave();
            }
        }

        void StartNextWave()
        {
            if (_currentWaveIndex >= _waveConfig.Waves.Count)
            {
                EventManager.GameStateChanged(Enums.GameState.GameOver);
                return;
            }

            var wave = _waveConfig.Waves[_currentWaveIndex];
            StartCoroutine(SpawnWaveRoutine(wave));
            _currentWaveIndex++;
        }

        private IEnumerator SpawnWaveRoutine(WaveModel wave)
        {
            float interval = 1f / wave.SpawnRatePerSecond;
            var tempList = wave.Enemy;
            var count = tempList.Count;
            for (int i = 0; i < count; i++)
            {
                var enemy = tempList[i];
                int enemyCount = enemy.Count;
                for (int j = 0; j < enemyCount; j++)
                {
                    _enemyPool.GetEnemy(enemy.EnemyType);
                    yield return new WaitForSeconds(interval);
                }
                _activeEnemyCount += enemyCount;
            }
        }
    }
}