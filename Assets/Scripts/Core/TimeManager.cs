using UnityEngine;

namespace TowerDefense.Core
{
    [DefaultExecutionOrder(-100)]
    public class TimeManager : MonoBehaviour
    {
        public static float TimeScale { get; private set; } = 1f;
        public static float DeltaTime { get; private set; }
        public static float ScaledDeltaTime => Time.deltaTime;

        private void Awake()
        {
            EventManager.OnTimeChanged += SetTimeScale;
        }

        private void Update()
        {
            DeltaTime = ScaledDeltaTime * TimeScale;
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