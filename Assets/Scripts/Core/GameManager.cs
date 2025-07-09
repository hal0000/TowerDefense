// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2024 Halil Mentes
// All rights reserved.
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

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
            }
        }
    }
}