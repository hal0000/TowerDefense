using PrimeTween;
using TowerDefense.Interface;
using UnityEngine;

namespace TowerDefense.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AlphaLoop : MonoBehaviour, IAnimatable
    {
        [Header("Animation Settings")] public float Duration = 1f;

        [Range(0f, 1f)] public float MinAlpha = 0.2f;
        [Range(0f, 1f)] public float MaxAlpha = 1f;
        public int Cycles = -1;
        public bool PlayOnStart;
        public Ease EaseType = Ease.Linear;
        [Range(0.1f, 5f)] public float SpeedMultiplier = 1f;

        private SpriteRenderer _graphic;
        private bool _isActive;
        private Tween _tween;

        private void Awake()
        {
            _graphic = GetComponent<SpriteRenderer>();
            _isActive = true;
        }

        private void Start()
        {
            if (PlayOnStart) StartAnimation();
        }

        private void OnEnable()
        {
            _isActive = true;
        }

        private void OnDisable()
        {
            _isActive = false;
            StopAnimation();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public bool IsAnimating { get; private set; }

        public bool IsActive => _isActive && _graphic != null;

        public void StartAnimation()
        {
            if (!IsActive || IsAnimating) return;
            IsAnimating = true;
            float t = Duration / SpeedMultiplier;
            _tween = Tween.Alpha(_graphic, MaxAlpha, MinAlpha, t, EaseType, -1, CycleMode.Yoyo);
        }

        public void StopAnimation()
        {
            if (!IsAnimating) return;
            IsAnimating = false;
            _tween.Stop();
            Tween.Alpha(_graphic, MaxAlpha, 0, 0, EaseType);
        }

        public void OnParentVisibilityChanged(bool isVisible)
        {
            _isActive = isVisible;
            if (isVisible)
            {
                if (!IsAnimating) StartAnimation();
            }
            else
            {
                if (IsAnimating) StopAnimation();
            }
        }
    }
}