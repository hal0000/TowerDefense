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
        public PlayerModel Model;
        
        public void Initialize(PlayerModel model)
        {
            Model = model;
            RegisterBindingContext();
            SetBindingData();
            EventManager.OnPlayerAction += PlayerAction;
        }

        private void PlayerAction(Enums.PlayerActions type, int value)
        {
            switch (type)
            {
                case Enums.PlayerActions.GetDamage:
                    Model.Health.Value -= 1;
                    if (Model.Health.Value <= 0)
                    {
                        EventManager.GameStateChanged(Enums.GameState.GameOver);
                    }
                    break;
                case Enums.PlayerActions.EnemyKilled:
                    Model.Gold.Value += value;
                    break;
                case Enums.PlayerActions.SpendGold:
                    Model.Gold.Value -= value;
                    break;
                case Enums.PlayerActions.NewLevel:
                    Model.Level.Value++;
                    break;
                case Enums.PlayerActions.GameOver:
                    Model.Health.Value = Model.DefaultHealth;
                    Model.Gold.Value = Model.DefaultGold;
                    Model.Level.Value = Model.DefaulLevel;
                    break;
            }
        }
#region BindingContextInterface
        public void SetBindingData()
        {
            // Gold = new Bindable<int>(_model.GoldData);
            // Level = new Bindable<int>(_model.LevelData);
            // Health = new Bindable<int>(_model.HealthData);

            Gold = Model.Gold;
            Health = Model.Health;
            Level = Model.Level;
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