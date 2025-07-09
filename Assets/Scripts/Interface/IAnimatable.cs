namespace TowerDefense.Interface
{
    public interface IAnimatable
    {
        void StartAnimation();
        void StopAnimation();
        bool IsAnimating { get; }
        bool IsActive { get; }
        void OnParentVisibilityChanged(bool isVisible);
    }
}