using UnityEngine;

namespace TowerDefense.Core
{
    public abstract class MonoBehaviourExtra : MonoBehaviour
    {
        protected bool _isPaused;

        private void Update()
        {
            if (_isPaused) return;
            Tick();
        }

        protected virtual void OnEnable()
        {
            EventManager.OnPauseChanged += HandlePauseChanged;
        }

        protected virtual void OnDisable()
        {
            EventManager.OnPauseChanged -= HandlePauseChanged;
        }

        protected abstract void Tick();

        protected virtual void HandlePauseChanged(bool paused)
        {
            _isPaused = paused;
            OnPauseUpdate(paused);
        }

        protected virtual void OnPauseUpdate(bool paused)
        {
        }
    }
}