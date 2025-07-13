using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.UI.Binding;

namespace TowerDefense.Controller
{
    public class PlayerController : IBindingContext
    {
        private Enums.GameState _lastState;
        public PlayerModel Model;
        public Bindable<int> Gold { get; private set; }
        public Bindable<int> Level { get; private set; }
        public Bindable<int> Health { get; private set; }

        public void Initialize(PlayerModel model)
        {
            Model = model;
            RegisterBindingContext();
            SetBindingData();
            EventManager.OnPlayerAction += PlayerAction;
            EventManager.OnGameStateChanged += GameStateChanged;
        }

        private void GameStateChanged(Enums.GameState type)
        {
            _lastState = type;
        }

        private void PlayerAction(Enums.PlayerActions type, int value)
        {
            switch (type)
            {
                case Enums.PlayerActions.GetDamage:
                    if (_lastState == Enums.GameState.Playing)
                    {
                        Model.Health.Value -= 1;
                        if (Model.Health.Value == 0)
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
            }
        }

        public void Restart()
        {
            Model.Health.Value = Model.DefaultHealth;
            Model.Gold.Value = Model.DefaultGold;
            Model.Level.Value = Model.DefaulLevel;
        }

        public void OnSoftDestroy()
        {
            EventManager.OnPlayerAction -= PlayerAction;
            UnregisterBindingContext();
        }

        #region BindingContextInterface

        public void SetBindingData()
        {
            Gold = Model.Gold;
            Health = Model.Health;
            Level = Model.Level;
        }

        public void RegisterBindingContext()
        {
            BindingContextRegistry.Register(GetType().Name, this);
        }

        public void UnregisterBindingContext()
        {
            BindingContextRegistry.Unregister(GetType().Name, this);
        }

        #endregion
    }
}