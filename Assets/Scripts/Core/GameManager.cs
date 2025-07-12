// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2024 Halil Mentes
// All rights reserved.
// --------------------------------------------------------------------------------------------------------------------

using TowerDefense.Network;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace TowerDefense.Core
{
    /// <summary>
    /// Central manager class that handles core game systems, scene management, and resource loading.
    /// Implements the Singleton pattern for global access to game systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {

        /// <summary>
        /// Singleton instance of the GameManager for global access.
        /// </summary>
        public static GameManager Instance { get; private set; }
        /// <summary>
        /// Mock Api class
        /// </summary>
        public Api Api;
        
        public BaseScene CurrentScene;
        private void Awake()
        {
            Application.targetFrameRate = 120;
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Api = new Api();
                EnhancedTouchSupport.Enable();
                TouchSimulation.Enable();
            }
        }
    }
}