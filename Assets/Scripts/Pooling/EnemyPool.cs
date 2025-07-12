using System;
using System.Collections.Generic;
using TowerDefense.Controller;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Pooling
{
    public sealed class EnemyPool : MonoBehaviour
    {
        public List<EnemyTypeDefinition> EnemyTypeDefinitions = new();

        private readonly Dictionary<Enums.EnemyType, Queue<BaseEnemyController>> _enemyPools = new();
        private readonly Dictionary<Enums.EnemyType, EnemyModel> _models = new();
        private IReadOnlyList<Vector3> _sharedPath;

        public void InitializePools(List<EnemyModel> models, IReadOnlyList<Vector3> path)
        {
            _enemyPools.Clear();
            _models.Clear();
            _sharedPath = path;

            for (int m = 0; m < models.Count; m++)
            {
                EnemyModel model = models[m];
                _models[model.Type] = model;

                EnemyTypeDefinition def = null;
                for (int j = 0; j < EnemyTypeDefinitions.Count; j++)
                {
                    if (EnemyTypeDefinitions[j].Type == model.Type)
                    {
                        def = EnemyTypeDefinitions[j];
                        break;
                    }
                }

                if (def == null)
                    throw new ArgumentOutOfRangeException(nameof(model.Type));

                Queue<BaseEnemyController> pool = new Queue<BaseEnemyController>(def.PoolSize);
                for (int i = 0; i < def.PoolSize; i++)
                {
                    BaseEnemyController e = Instantiate(def.Prefab, transform);
                    e.gameObject.SetActive(false);
                    e.Initialize(model, path);
                    pool.Enqueue(e);
                }

                _enemyPools[model.Type] = pool;
            }
        }

        public BaseEnemyController GetEnemy(Enums.EnemyType type)
        {
            if (!_enemyPools.TryGetValue(type, out Queue<BaseEnemyController> pool)) return null;
            if (pool.Count > 0)
            {
                BaseEnemyController e = pool.Dequeue();
                e.OnSpawn();
                e.gameObject.SetActive(true);
                return e;
            }

            EnemyTypeDefinition def = null;
            for (int j = 0; j < EnemyTypeDefinitions.Count; j++)
            {
                if (EnemyTypeDefinitions[j].Type != type) continue;
                def = EnemyTypeDefinitions[j];
                break;
            }

            if (def == null) return null;

            EnemyModel model = _models[type];
            BaseEnemyController eNew = Instantiate(def.Prefab, transform);
            eNew.Initialize(model, _sharedPath);
            eNew.OnSpawn();
            eNew.gameObject.SetActive(true);
            return eNew;
        }

        public void ReturnEnemy(Enums.EnemyType type, BaseEnemyController enemy)
        {
            if (!_enemyPools.TryGetValue(type, out Queue<BaseEnemyController> pool))
            {
                Destroy(enemy.gameObject);
                return;
            }

            pool.Enqueue(enemy);
            enemy.gameObject.SetActive(false);
        }

        [Serializable]
        public class EnemyTypeDefinition
        {
            public Enums.EnemyType Type;
            public BaseEnemyController Prefab;
            public int PoolSize = 10;
        }
    }
}