using System;

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
    }
}