// TowerDefense/Controller/TowerController.cs

using TowerDefense.Core;
using TowerDefense.Interface;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    /// <summary>
    /// Wraps a TowerModel on your prefab and exposes its pre-decoded byte-grid footprint.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class TowerController : MonoBehaviourExtra, ITower
    {
        public TowerModel Model;

        /// <summary>Number of rows in the footprint grid.</summary>
        public int Rows => Model.Rows;

        /// <summary>Number of cols in the footprint grid.</summary>
        public int Cols => Model.Cols;

        /// <summary>
        /// Fast access into the flat byte[]: returns 1 for occupied, 0 for empty.
        /// </summary>
        public byte GetCell(int r, int c) => Model.GetCell(r, c);

        // other stats...
        public int Range => Model.Range;
        public int Damage => Model.Damage;
        public int Level => Model.Level;
        public int FireRate => Model.FireRate;

        /// <summary>
        /// Tracks this tower’s grid position (packed int).
        /// </summary>
        public int PackedPosition
        {
            get => Model.PackedPosition;
            set => Model.SetPosition(value);
        }
        
        public void Initialize(TowerModel model)
        {
            Model = model;

        }

        protected override void Tick()
        {
            
        }

        public void GetDamage()
        {
            throw new System.NotImplementedException();
        }

        public void Shoot()
        {
            throw new System.NotImplementedException();
        }

        public void Sell()
        {
            throw new System.NotImplementedException();
        }
    }
}