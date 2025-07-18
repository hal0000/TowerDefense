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
                EnemyModel proto = models[m];
                _models[proto.Type] = proto;

                EnemyTypeDefinition def = null;
                for (int j = 0; j < EnemyTypeDefinitions.Count; j++)
                {
                    if (EnemyTypeDefinitions[j].Type != proto.Type) continue;
                    def = EnemyTypeDefinitions[j];
                    break;
                }
                if (def == null) throw new ArgumentOutOfRangeException($"No definition for {proto.Type}");

                var pool = new Queue<BaseEnemyController>(def.PoolSize);
                for (int i = 0; i < def.PoolSize; i++)
                {
                    var enemy = Instantiate(def.Prefab, transform);
                    enemy.gameObject.SetActive(false);
                    var instanceModel = new EnemyModel(proto); 
                    enemy.Initialize(instanceModel, path);
                    pool.Enqueue(enemy);
                }

                _enemyPools[proto.Type] = pool;
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