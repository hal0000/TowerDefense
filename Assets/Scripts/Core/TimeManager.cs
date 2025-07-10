using UnityEngine;

namespace TowerDefense.Core
{
    [DefaultExecutionOrder(-100)]
    public class TimeManager : MonoBehaviour
    {
        public static float TimeScale { get; private set; } = 1f;
        public static float DeltaTime { get; private set; }
        public static float UnscaledDeltaTime => Time.unscaledDeltaTime;
        public void SetTimeScale(float scale) => TimeScale = scale;
        void Update() => DeltaTime = UnscaledDeltaTime * TimeScale;

        private void Awake() => EventManager.OnTimeChanged += SetTimeScale;
        private void OnDestroy() => EventManager.OnTimeChanged -= SetTimeScale;
    }
}