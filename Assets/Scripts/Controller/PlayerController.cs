using System;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class PlayerController : IBindingContext
    {
        public Bindable<int> Gold { get; private set; }
        public Bindable<int> Level { get; private set; }
        public Bindable<int> Health { get; private set; }
        private PlayerModel _model;
        
        public void Initialize(PlayerModel model)
        {
            _model = model;
            RegisterBindingContext();
            SetBindingData();
            EventManager.OnPlayerAction += PlayerAction;
        }

        private void PlayerAction(Enums.PlayerActions type, int value)
        {
            switch (type)
            {
                case Enums.PlayerActions.GetDamage:
                    _model.Health.Value -= value;
                    if (_model.Health.Value <= 0)
                    {
                        //EventManager.gamestate changed to gameover
                    }
                    break;
                case Enums.PlayerActions.SpendGold:
                    _model.Gold.Value -= value;
                    break;
                case Enums.PlayerActions.GameOver:
                    _model.Health.Value -= _model.DefaultHealth;
                    _model.Gold.Value -= _model.DefaultGold;
                    _model.Level.Value -= _model.DefaulLevel;
                    break;
            }
        }
#region BindingContextInterface
        public void SetBindingData()
        {
            // Gold = new Bindable<int>(_model.GoldData);
            // Level = new Bindable<int>(_model.LevelData);
            // Health = new Bindable<int>(_model.HealthData);

            Gold = _model.Gold;
            Health = _model.Health;
            Level = _model.Level;
        }
        public void RegisterBindingContext() => BindingContextRegistry.Register(GetType().Name, this);
        public void UnregisterBindingContext() => BindingContextRegistry.Unregister(GetType().Name, this);
        #endregion
        public void OnSoftDestroy()
        {
            EventManager.OnPlayerAction -= PlayerAction;
            UnregisterBindingContext();
        }
    }
}