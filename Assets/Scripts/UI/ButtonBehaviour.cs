// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2024 Halil Mentes
// All rights reserved.
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using PrimeTween;
using TowerDefense.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TowerDefense.UI
{
    public class ButtonBehaviour : UIElement, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool ButtonDisabled;
        public UnityEvent ClickAction = new();
        public UnityEvent<UnityAction> Disabled = new();
        public UnityEvent<UnityAction> Enabled = new();
        public bool NoAnimation;

        [Header("Hold Settings")] public bool EnableHold;

        public float HoldInvokeInterval = 0.1f;
        private readonly float _animationDuration = 0.1f;

        private readonly float _scaleDownSize = 0.85f;
        private Coroutine _holdCoroutine;

        private Vector3 _originalScale;
        private Tween _scaleTween;

        private void Start()
        {
            _originalScale = transform.localScale;
            if (ButtonDisabled)
                Disabled?.Invoke(null);
        }

        public void OnPointerClick(PointerEventData data)
        {
            if (ButtonDisabled) return;
            if (data.button == PointerEventData.InputButton.Left) ClickAction.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ButtonDisabled) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (EnableHold)
            {
                StopHold();
                _holdCoroutine = StartCoroutine(HoldRoutine());
            }

            if (NoAnimation) return;
            _scaleTween.Complete();
            Transform tr = transform;
            Vector3 trLocalScale = tr.localScale;
            _scaleTween = Tween.Scale(tr, trLocalScale, _originalScale * _scaleDownSize, _animationDuration, Ease.InSine);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ButtonDisabled) return;
            if (EnableHold) StopHold();
            if (NoAnimation) return;
            _scaleTween.Complete();
            Transform tr = transform;
            _scaleTween = Tween.Scale(tr, tr.localScale, _originalScale, _animationDuration, Ease.OutSine);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (ButtonDisabled) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (EnableHold) StopHold();
            if (NoAnimation) return;
            _scaleTween.Complete();
            Transform tr = transform;
            _scaleTween = Tween.Scale(tr, tr.localScale, _originalScale, _animationDuration, Ease.OutSine);
        }

        public void SetDisabled()
        {
            ButtonDisabled = true;
            Disabled?.Invoke(null);
        }

        public void SetEnabled()
        {
            ButtonDisabled = false;
            Enabled?.Invoke(null);
        }

        private IEnumerator HoldRoutine()
        {
            while (true)
            {
                ClickAction.Invoke();
                yield return new WaitForSeconds(HoldInvokeInterval);
            }
        }

        private void StopHold()
        {
            if (_holdCoroutine == null) return;
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }

        public void Pause()
        {
            EventManager.SetPause(true);
        }

        public void Continue()
        {
            EventManager.SetPause(false);
        }
    }
}