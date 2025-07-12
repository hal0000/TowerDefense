using System;
using System.Runtime.CompilerServices;
using TowerDefense.Core;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense.Model
{
    [Serializable]
    public class TowerModel : BaseModel, ISerializationCallbackReceiver
    {
        public Bindable<string> NameBinder { get; } = new();
        public Bindable<int> GoldBinder { get; } = new();


        /// <summary>
        ///     Number of rows in the footprint.
        /// </summary>
        public int Rows => _rows;

        /// <summary>
        ///     Number of cols in the footprint.
        /// </summary>
        public int Cols => _cols;

        /// <summary>
        ///     Tower’s current grid position, packed.
        /// </summary>
        public int PackedPosition => _packedPosition;

        /// <summary>
        ///     Unpacked X
        /// </summary>
        public int X => CoordPacker.UnpackX(_packedPosition);

        /// <summary>
        ///     Unpacked Y
        /// </summary>
        public int Y => CoordPacker.UnpackY(_packedPosition);

        public void OnAfterDeserialize()
        {
            // build our fast-access byte[] from the string rows
            GoldBinder.Value = Gold;
            NameBinder.Value = Name;
            _rows = Footprint?.Length ?? 0;
            _cols = _rows > 0 ? Footprint[0].Length : 0;
            _grid = new byte[_rows * _cols];

            for (int r = 0; r < _rows; r++)
            {
                string row = Footprint[r];
                for (int c = 0; c < _cols; c++)
                {
                    _grid[r * _cols + c] = (byte)(row[c] == '1' ? 1 : 0);
                }
            }
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        ///     Get cell value (0 or 1) at (r,c).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetCell(int r, int c)
        {
            return _grid[r * _cols + c];
        }

        /// <summary>
        ///     Set tower’s position on the grid.
        /// </summary>
        public void SetPosition(int packed)
        {
            _packedPosition = packed;
        }

        #region JSONKEYS

        public int Index;
        public string Name;
        public int Gold;
        public string[] Footprint;
        public int Range;
        public int Damage;
        public int Level;
        public int FireRate;

        #endregion

        #region RUNTIME

        [NonSerialized] private int _rows;
        [NonSerialized] private int _cols;
        [NonSerialized] private byte[] _grid; // flat (row*cols + col)
        [NonSerialized] private int _packedPosition;

        #endregion
    }

    [Serializable]
    public class TowerModelList
    {
        public TowerModel[] Templates;
    }
}