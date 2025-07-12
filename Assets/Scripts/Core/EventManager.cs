using System;
using TowerDefense.Model;

namespace TowerDefense.Core
{
    /// <summary>
    /// Central event management system that handles game-wide events
    /// Provides type-safe event handling and subscription mechanisms.
    /// </summary>
    public static class EventManager
    {
        
        /// <summary>
        /// Gets the current pause state of the game.
        /// </summary>
        public static bool IsPaused { get; private set; }

        /// <summary>
        /// Event triggered when the game's pause state changes.
        /// </summary>
        public static event Action<bool> OnPauseChanged;

        /// <summary>
        /// Sets the game's pause state and notifies subscribers.
        /// </summary>
        /// <param name="isPaused">The new pause state</param>
        public static void SetPause(bool isPaused)
        {
            IsPaused = isPaused;
            OnPauseChanged?.Invoke(isPaused);
        }
        
        /// <summary>
        /// Delegate for handling nickname change events.
        /// </summary>
        public delegate void TimeMultiplierChanged(float value);
        /// <summary>
        /// Event triggered when a TimeMultiplier change is requested.
        /// </summary>
        public static event TimeMultiplierChanged OnTimeChanged;

        /// <summary>
        /// Notifies subscribers that a TimeMultiplier change is needed.
        /// </summary>
        public static void TimeMultiplierEventNeeded(float value)
        {
            OnTimeChanged?.Invoke(value);
        }

        #region PlayerActions

        /// <summary>
        /// Delegate for handling player events.
        /// </summary>
        public delegate void PlayerAction(Enums.PlayerActions type, int value);

        /// <summary>
        /// Event triggered when a player make an action
        /// </summary>
        public static event PlayerAction OnPlayerAction;
        

        /// <summary>
        /// Notifies subscribers that a player did something
        /// </summary>
        public static void PlayerDidSomething(Enums.PlayerActions type, int value = 0)
        {
            OnPlayerAction?.Invoke(type,value);
        }
        #endregion
        
        #region GameStateActions

        /// <summary>
        /// Delegate for handling player events.
        /// </summary>
        public delegate void GameState(Enums.GameState type);

        /// <summary>
        /// Event triggered when a player make an action
        /// </summary>
        public static event GameState OnGameStateChanged;
        

        /// <summary>
        /// Notifies subscribers that a player did something
        /// </summary>
        public static void GameStateChanged(Enums.GameState type)
        {
            OnGameStateChanged?.Invoke(type);
        }
        #endregion
        /// <summary>
        /// Delegate for handling notification events.
        /// </summary>
        /// <param name="type">The type of notification to display</param>
        public delegate void NotificationEvent(Enums.NotificationType type);
        
        /// <summary>
        /// Event triggered when a new notification needs to be displayed.
        /// </summary>
        public static event NotificationEvent OnNewNotification;

        /// <summary>
        /// Notifies subscribers that a new notification should be displayed.
        /// </summary>
        /// <param name="type">The type of notification to display</param>
        public static void NewNotificationHappened(Enums.NotificationType type)
        {
            OnNewNotification?.Invoke(type);
        }
    }
}