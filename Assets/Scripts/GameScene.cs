using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense
{
    public class GameScene : MonoBehaviour, IBindingContext
    {
        public GridManager GridManager; 
        public Bindable<string> State { get; private set; }
        [HideInInspector] public Enums.GameState GameState = Enums.GameState.Nothing;
        public List<GameObject> Towers = new List<GameObject>();
        private void Awake()
        {
            RegisterBindingContext();
        }

        public void Start()
        {
            GameState = Enums.GameState.Nothing;
            SetBindingData();

        }

        public void StartGame()
        {
            GridManager.BuildGrid(30, 30);
            GameState = Enums.GameState.Preparing;
            State.Value = GameState.ToString();
        }

        public void SetBindingData()
        {
            State = new Bindable<string>(GameState.ToString());
        }

        public void RegisterBindingContext()
        {
            BindingContextRegistry.Register(GetType().Name, this);
        }

        public void UnregisterBindingContext()
        {
            BindingContextRegistry.Unregister(GetType().Name, this);
        }
    }
}
