using System;
using System.Runtime.CompilerServices;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Model
{
    [Serializable]
    public class TowerModel : BaseModel, ISerializationCallbackReceiver
    {
#region JSONKEYS
        public int Index;
        public string Name;
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


        /// <summary>
        /// Number of rows in the footprint.
        /// </summary>
        public int Rows => _rows;

        /// <summary>
        /// Number of cols in the footprint.
        /// </summary>
        public int Cols => _cols;

        /// <summary>
        /// Get cell value (0 or 1) at (r,c).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetCell(int r, int c) => _grid[r * _cols + c];

        /// <summary>
        /// Tower’s current grid position, packed.
        /// </summary>
        public int PackedPosition => _packedPosition;

        /// <summary>
        /// Unpacked X
        /// </summary>
        public int X => CoordPacker.UnpackX(_packedPosition);

        /// <summary>
        /// Unpacked Y
        /// </summary>
        public int Y => CoordPacker.UnpackY(_packedPosition);

        /// <summary>
        /// Set tower’s position on the grid.
        /// </summary>
        public void SetPosition(int packed) => _packedPosition = packed;

        public void OnAfterDeserialize()
        {
            // build our fast-access byte[] from the string rows
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
        public void OnBeforeSerialize() {}
    }

    [Serializable]
    public class TowerModelList
    {
        public TowerModel[] Templates;
    }
}