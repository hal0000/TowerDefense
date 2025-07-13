using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using TowerDefense.UI.Binding;

namespace TowerDefense.Controller
{
    /// <summary>
    ///     Wraps a TowerModel on your prefab and exposes its pre-decoded byte-grid footprint.
    /// </summary>
    public class TowerController : MonoBehaviourExtra, ITower
    {
        public TowerModel Model;
        public WeaponController WeaponController;


        public Bindable<int> TowerState = new();
        public List<int> OccupiedCells = new();

        public bool CanIEdit;

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

        public void Upgrade()
        {
            Model.Level++;
            Model.Range += 1;
            Model.Damage += 5;
            WeaponController.Initialize(Model);
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
            WeaponController.Initialize(model);
        }

        protected override void Tick()
        {
        }

        public void CanvasHandler(bool show)
        {
            WeaponController._col.enabled = !show;
            //GridInputHandler.Instance.StartMove(this);
        }
    }
}