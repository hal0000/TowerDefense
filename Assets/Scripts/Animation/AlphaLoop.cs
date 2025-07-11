using PrimeTween;
using TowerDefense.Core;
using TowerDefense.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AlphaLoop : MonoBehaviour, IAnimatable
    {
        [Header("Animation Settings")]
        public float Duration = 1f;
        [Range(0f,1f)] public float MinAlpha = 0.2f;
        [Range(0f,1f)] public float MaxAlpha = 1f;
        public int Cycles = -1;
        public bool PlayOnStart;
        public Ease EaseType = Ease.Linear;
        [Range(0.1f,5f)] public float SpeedMultiplier = 1f;

        SpriteRenderer _graphic;
        Tween _tween;
        bool _isAnimating;
        bool _isActive;

        void Awake()
        {
            _graphic = GetComponent<SpriteRenderer>();
            _isActive = true;
        }

        void Start()
        {
            if (PlayOnStart) StartAnimation();
        }

        public bool IsAnimating => _isAnimating;
        public bool IsActive => _isActive && _graphic != null;

        public void StartAnimation()
        {
            if (!IsActive || _isAnimating) return;
            _isAnimating = true;
            float t = Duration / SpeedMultiplier;
            _tween = Tween.Alpha(_graphic, MaxAlpha, MinAlpha, t, EaseType,-1,CycleMode.Yoyo);
        }

        public void StopAnimation()
        {
            if (!_isAnimating) return;
            _isAnimating = false;
            _tween.Stop();
        }

        public void OnParentVisibilityChanged(bool isVisible)
        {
            _isActive = isVisible;
            if (isVisible)
            {
                if (!_isAnimating) StartAnimation();
            }
            else
            {
                if (_isAnimating) StopAnimation();
            }
        }

        void OnDisable()
        {
            _isActive = false;
            StopAnimation();
        }

        void OnEnable() => _isActive = true;
        void OnDestroy() => StopAnimation();
    }
}