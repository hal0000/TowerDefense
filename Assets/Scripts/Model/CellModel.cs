using System;

namespace TowerDefense.Model
{
    /// <summary>
    /// Represents the state of a grid cell using a single packed int for X and Y,
    /// plus bit-flags for occupancy/path.
    /// </summary>
    public class CellModel
    {
        public string Name { get; }

        // high 16 bits = X, low 16 bits = Y
        private int _packedCoord;

        private CellFlags _flags;

        /// <summary>–32,768 … +32,767</summary>
        public int X => _packedCoord >> 16;

        /// <summary>–32,768 … +32,767</summary>
        public int Y => (short)(_packedCoord & 0xFFFF);
        public bool IsOccupied => (_flags & CellFlags.Occupied) != 0;
        public bool IsPath => (_flags & CellFlags.Path) != 0;

        public CellFlags Flags => _flags;
        public CellModel(string name, int x, int y)
        {
            Name = name;
            _packedCoord = (x << 16) | (y & 0xFFFF);
            _flags = CellFlags.None;
        }
        public void SetOccupied(bool occupied)
        {
            if (occupied) _flags |= CellFlags.Occupied;
            else _flags &= ~CellFlags.Occupied;
        }

        public void SetPath(bool path)
        {
            if (path) _flags |= CellFlags.Path;
            else _flags &= ~CellFlags.Path;
        }

        /// <summary>
        /// Move the model to a different grid cell.
        /// </summary>
        public void MoveTo(int newX, int newY)
        {
            _packedCoord = (newX << 16) | (newY & 0xFFFF);
        }
        
        /// <summary>
        /// Cell Status
        /// </summary>
        [Flags]
        public enum CellFlags
        {
            None = 0,
            Occupied = 1 << 0,
            Path = 1 << 1,
            Start = 1 << 2,
            Finish = 1 << 3,
            // for features, vs.
        }
        
    }
}