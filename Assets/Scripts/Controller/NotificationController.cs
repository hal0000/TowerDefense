using System;
using System.Collections;
using TMPro;
using TowerDefense.Core;
using TowerDefense.Model;
using TowerDefense.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Controller
{
    /// <summary>
    /// Manages the display of in-game notifications, handling different notification types,
    /// animations, and automatic dismissal after a delay.
    /// </summary>
    public class NotificationController : UIElement
    {
        /// <summary>
        /// The background rectangle that contains the notification text.
        /// </summary>
        [SerializeField] private RectTransform _backgroundRect;

        /// <summary>
        /// The text component that displays the notification message.
        /// </summary>
        [SerializeField] private TextMeshProUGUI _notificationText;

        /// <summary>
        /// The horizontal padding to add around the notification text.
        /// </summary>
        [SerializeField] private float _horizontalPadding = 20f;

        /// <summary>
        /// The current type of notification being displayed.
        /// </summary>
        public Enums.NotificationType Type;

        /// <summary>
        /// Coroutine reference for managing the notification display duration.
        /// </summary>
        private Coroutine _co;

        /// <summary>
        /// The last notification type that was displayed, used to prevent duplicate notifications.
        /// </summary>
        private Enums.NotificationType _lastType;

        /// <summary>
        /// Initializes the notification controller by subscribing to notification events.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            EventManager.OnNewNotification += SetNotificationValue;
        }

        /// <summary>
        /// Cleans up event subscriptions when the component is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.OnNewNotification -= SetNotificationValue;
        }

        /// <summary>
        /// Sets the notification text based on the notification type and starts the display routine.
        /// </summary>
        /// <param name="value">The type of notification to display</param>
        private void SetNotificationValue(Enums.NotificationType value)
        {
            if (_lastType == value) return;
            _lastType = value;
            switch (value)
            {
                case Enums.NotificationType.NotEnoughGold:
                    _notificationText.text = "NOT Enough Gold!";
                    break;
                case Enums.NotificationType.TowerPositionIsNotValid:
                    _notificationText.text = "Tower Position Is NOT Valid!";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            UpdateBackgroundWidth();
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(NotificationRoutine());
        }

        /// <summary>
        /// Shows the notification with animations.
        /// </summary>
        public override void Show()
        {
            base.Show();
            ShowAnimations();
            OnShow.Invoke();
        }

        /// <summary>
        /// Hides the notification with animations.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            HideAnimations();
            OnHide.Invoke();
        }

        /// <summary>
        /// Coroutine that manages the notification display duration and automatic dismissal.
        /// </summary>
        private IEnumerator NotificationRoutine()
        {
            Show();
            yield return new WaitForSeconds(3);
            Hide();
            _lastType = Enums.NotificationType.Nothing;
        }

        /// <summary>
        /// Updates the background width to match the notification text width plus padding.
        /// </summary>
        private void UpdateBackgroundWidth()
        {
            // Force the TextMeshProUGUI to update its layout (in case it hasn't been updated yet).
            LayoutRebuilder.ForceRebuildLayoutImmediate(_notificationText.rectTransform);
            // Use the preferredWidth property to get the ideal width for the text.
            float textWidth = _notificationText.preferredWidth;
            // Set the background width to the text width plus the horizontal padding.
            _backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth + _horizontalPadding);
        }
    }
}