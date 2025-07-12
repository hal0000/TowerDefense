using System;

namespace TowerDefense.Model
{
    [Serializable]
    public class EnemyModel : BaseModel
    {
        public Enums.EnemyType Type;
        public string Name;
        public float Speed;
        public float TurnRate;
        public float TurnRadius;
        public int Damage;
        public int Health;
        public int Gold;
    }

    [Serializable]
    public class EnemyModelList
    {
        public EnemyModel[] Enemies;
    }
}