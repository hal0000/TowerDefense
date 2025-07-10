// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2024 Halil Mentes
// All rights reserved.
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TowerDefense.Core;
using TowerDefense.Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

namespace TowerDefense.UI
{
    public sealed class TowerButton : MonoBehaviour
    {
        private GameScene _gameScene;
        public TowerModel _model;
        public TextBinder Name;
        public TextBinder Gold;

        public int Index;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(TowerModel model, int index)
        {
            _model = model;
            Index = index;
            Name.ListIndex = index;
            Gold.ListIndex = index;
            if (GameManager.Instance.CurrentScene is GameScene scene) _gameScene = scene;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrySelect()
        {
            _gameScene.SelectTower(_model.Index);
        }
    }
}