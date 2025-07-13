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

        public EnemyModel() { }
        public EnemyModel(EnemyModel other)
        {
            Type = other.Type;
            Name = other.Name;
            Speed = other.Speed;
            TurnRate = other.TurnRate;
            TurnRadius = other.TurnRadius;
            Damage = other.Damage;
            Health = other.Health;
            Gold = other.Gold;
        }
    }

    [Serializable]
    public class EnemyModelList
    {
        public EnemyModel[] Enemies;
    }
}