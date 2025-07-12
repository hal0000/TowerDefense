using System;
using System.Collections.Generic;

namespace TowerDefense.Model
{
    [Serializable]
    public class WaveModel
    {
        public string Name;
        public List<SpawnModel> Enemy;
        public float SpawnRatePerSecond = 1f;
    }

    [Serializable]
    public class SpawnModel
    {
        public int Count;
        public Enums.EnemyType EnemyType;
    }
}