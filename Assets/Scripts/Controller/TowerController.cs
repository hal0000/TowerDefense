using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense.Controller
{
    /// <summary>
    ///     Wraps a TowerModel on your prefab and exposes its pre-decoded byte-grid footprint.
    /// </summary>
    public class TowerController : MonoBehaviourExtra, ITower
    {
        public TowerModel Model;
        public WeaponController WeaponController;

        [SerializeField] private Canvas _canvas;

        public Bindable<int> TowerState = new();
        public List<int> OccupiedCells = new();

        public bool CanIEdit;

        /// <summary>Number of rows in the footprint grid.</summary>
        public int Rows => Model.Rows;

        /// <summary>Number of cols in the footprint grid.</summary>
        public int Cols => Model.Cols;

        // other stats...
        public int Range => Model.Range;
        public int Damage => Model.Damage;
        public int Level => Model.Level;
        public int FireRate => Model.FireRate;

        /// <summary>
        ///     Tracks this tower’s grid position (packed int).
        /// </summary>
        public int PackedPosition
        {
            get => Model.PackedPosition;
            set => Model.SetPosition(value);
        }

        public void Bauen(List<int> positions)
        {
            CanvasHandler(false);
            CanIEdit = true;
        }

        public void Abbrechen()
        {
            CanvasHandler(false);
            Destroy(gameObject);
        }

        /// <summary>
        ///     Fast access into the flat byte[]: returns 1 for occupied, 0 for empty.
        /// </summary>
        public byte GetCell(int r, int c)
        {
            return Model.GetCell(r, c);
        }

        public void Initialize(TowerModel model)
        {
            Model = model;
            TowerState.Value = 0;
            _canvas.gameObject.SetActive(true);
            WeaponController.FireRate = model.FireRate;
            WeaponController.Damage = model.Damage;
        }

        protected override void Tick()
        {
        }

        public void CanvasHandler(bool show)
        {
            _canvas.gameObject.SetActive(show);
            //GridInputHandler.Instance.StartMove(this);
        }
    }
}