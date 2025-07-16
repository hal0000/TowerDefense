using TowerDefense.Controller;

namespace TowerDefense.Pooling
{
    public sealed class BulletPool : BasePool<BulletController>
    {
        public BulletController GetBullet() => Get();
        public void ReturnBullet(BulletController bullet)
        {
            Return(bullet);
        }
        protected override void InitializeItem(BulletController item) { }
        
        protected override void OnGet(BulletController item)
        {
            item.OnSpawn();
        }
        protected override void OnReturn(BulletController item) { }
    }
}