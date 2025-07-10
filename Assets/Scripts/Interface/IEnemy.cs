namespace TowerDefense.Interface
{
    public interface IEnemy
    {
        public void GetDamage();
        public void ApplyDamage();
        public void Die();
        public void Win();
    }
}