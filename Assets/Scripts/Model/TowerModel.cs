using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TowerDefense.Core;
using TowerDefense.UI.Binding;
using UnityEngine;

namespace TowerDefense.Model
{
    [Serializable]
    public class TowerModel : BaseModel, ISerializationCallbackReceiver
    {
        // --- Binding ---
        public Bindable<string> NameBinder { get; } = new();
        public Bindable<int>    GoldBinder { get; } = new();

        // --- JSON-serializable fields ---
        public int Index;
        public string Name;
        public int Gold;

        /// <summary>
        /// Packed footprint: 
        ///   [0] = header  (Rows<<16 | Cols)
        ///   [1 und weiter] = bitmask for each row (LSB = column 0)
        /// /// </summary>
        public int[] FootprintPacked;
        public int Range;
        public int Damage;
        public int Level;
        public int FireRate;

        /// <summary>
        /// Tower’s current grid position, packed via CoordPacker.
        /// </summary>
        public int PackedPosition;

        // --- Runtime-only caches (non-serialized) ---
        [NonSerialized] private int _rows;
        [NonSerialized] private int _cols;
        public int Rows => _rows;
        public int Cols => _cols;
        public int X => CoordPacker.UnpackX(PackedPosition);
        public int Y => CoordPacker.UnpackY(PackedPosition);

        public void OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            NameBinder.Value = Name;
            GoldBinder.Value = Gold;

            if (FootprintPacked == null || FootprintPacked.Length == 0)
            {
                // default 1x1 boş
                FootprintPacked = new[] { (1 << 16) | 1, 0 };
            }

            int header = FootprintPacked[0];
            _rows = CoordPacker.UnpackX(header);
            _cols = CoordPacker.UnpackY(header);

            // satır maskelerini kontrol et
            if (FootprintPacked.Length != _rows + 1)
            {
                var newArr = new int[_rows + 1];
                Array.Copy(FootprintPacked, newArr, Math.Min(FootprintPacked.Length, _rows+1));
                FootprintPacked = newArr;
            }
        }

        /// <summary>
        /// Get cell value (0 or 1) at (r,c) by unpacking the bitmask.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetCell(int r, int c) => (byte)((FootprintPacked[1 + r] >> c) & 1);

        /// <summary>
        /// Set cell (r,c) on/off in the bitmask.
        /// </summary>
        public void SetCell(int r, int c, byte val)
        {
            int idx  = 1 + r; // index of that row’s mask in the array
            int mask = FootprintPacked[idx];
            if (val == 1) mask |=  (1 << c);
            else mask &= ~(1 << c);
            FootprintPacked[idx] = mask;
        }

        /// <summary>
        /// Set tower’s packed grid position.
        /// </summary>
        public void SetPosition(int packed)
        {
            PackedPosition = packed;
        }
    }

    [Serializable]
    public class TowerModelList
    {
        public List<TowerModel> Templates;
    }
}