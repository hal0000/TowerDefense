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

            if (_activeEnemyCount == 0) EventManager.GameStateChanged(Enums.GameState.Preparing);
        }

        private void OnGameStateChanged(Enums.GameState state)
        {
            if (state == Enums.GameState.Playing) StartNextWave();
        }

        private void StartNextWave()
        {
            if (_currentWaveIndex >= _waveConfig.Waves.Count)
            {
                EventManager.GameStateChanged(Enums.GameState.GameOver);
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
            foreach (var spawn in wave.Enemy)
                _activeEnemyCount += spawn.Count;
            for (int i = 0; i < count; i++)
            {
                SpawnModel enemy = tempList[i];
                int enemyCount = enemy.Count;
                for (int j = 0; j < enemyCount; j++)
                {
                    _enemyPool.GetEnemy(enemy.EnemyType);
                    yield return new WaitForSeconds(interval);
                }
            }
        }
    }
}