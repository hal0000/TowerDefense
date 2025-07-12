// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2024 Halil Mentes
// All rights reserved.
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using TowerDefense.Core;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.UI
{
    public sealed class TowerButton : MonoBehaviour
    {
        public TowerModel _model;
        public TextBinder Name;
        public TextBinder Gold;

        public int Index;
        private GameScene _gameScene;

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