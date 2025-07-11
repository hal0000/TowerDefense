using UnityEngine;

namespace TowerDefense.Core
{
    [DefaultExecutionOrder(-100)]
    public class TimeManager : MonoBehaviour
    {
        public static float TimeScale { get; private set; } = 1f;
        public static float DeltaTime { get; private set; }
        public static float UnscaledDeltaTime => Time.unscaledDeltaTime;

        private void Awake()
        {
            EventManager.OnTimeChanged += SetTimeScale;
        }

        private void Update()
        {
            DeltaTime = UnscaledDeltaTime * TimeScale;
        }

        private void OnDestroy()
        {
            EventManager.OnTimeChanged -= SetTimeScale;
        }

        private void SetTimeScale(float scale)
        {
            TimeScale = scale;
        }
    }
}