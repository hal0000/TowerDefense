using System;

namespace TowerDefense.Model
{
    /// <summary>
    ///     Represents the state of a grid cell using a single packed int for X and Y,
    ///     plus bit-flags for occupancy/path.
    /// </summary>
    public class CellModel
    {
        /// <summary>
        ///     Cell Status
        /// </summary>
        [Flags]
        public enum CellFlags
        {
            None = 0,
            Occupied = 1 << 0,
            Path = 1 << 1,
            Start = 1 << 2,

            Finish = 1 << 3
            // for features, vs.
        }

        // high 16 bits = X, low 16 bits = Y
        private int _packedCoord;

        public CellModel(string name, int x, int y)
        {
            Name = name;
            _packedCoord = (x << 16) | (y & 0xFFFF);
            Flags = CellFlags.None;
        }

        public string Name { get; }

        /// <summary>–32,768 … +32,767</summary>
        public int X => _packedCoord >> 16;

        /// <summary>–32,768 … +32,767</summary>
        public int Y => (short)(_packedCoord & 0xFFFF);

        public bool IsOccupied => (Flags & CellFlags.Occupied) != 0;
        public bool IsPath => (Flags & CellFlags.Path) != 0;

        public CellFlags Flags { get; private set; }

        public void SetOccupied(bool occupied)
        {
            if (occupied) Flags |= CellFlags.Occupied;
            else Flags &= ~CellFlags.Occupied;
        }

        public void SetPath(bool path)
        {
            if (path) Flags |= CellFlags.Path;
            else Flags &= ~CellFlags.Path;
        }

        /// <summary>
        ///     Move the model to a different grid cell.
        /// </summary>
        public void MoveTo(int newX, int newY)
        {
            _packedCoord = (newX << 16) | (newY & 0xFFFF);
        }
    }
}