namespace TowerDefense.Interface
{
    public interface IBindingContext
    {
        public void SetBindingData();
        public void RegisterBindingContext();
        public void UnregisterBindingContext();
    }
}