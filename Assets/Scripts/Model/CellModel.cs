using System;

namespace TowerDefense.Model
{
    public class CellModel
    {
        [Flags]
        public enum CellFlags : uint
        {
            None = 0,
            Occupied = 1 << 0,
            Path = 1 << 1,
            Start = 1 << 2,
            Finish = 1 << 3
        }

        private int _packedCoord;

        public CellModel(string name, int x, int y)
        {
            Name = name;
            _packedCoord = (x << 16) | (y & 0xFFFF);
            Flags = CellFlags.None;
        }

        public CellFlags Flags { get; private set; }

        public string Name { get; }

        /// <summary>–32 768 - +32 767</summary>
        public int X => _packedCoord >> 16;

        /// <summary>–32 768 - +32 767</summary>
        public int Y => (short)(_packedCoord & 0xFFFF);

        public bool IsOccupied => (Flags & CellFlags.Occupied) != 0;
        public bool IsPath => (Flags & CellFlags.Path) != 0;

        public void SetOccupied(bool occ)
        {
            Flags = occ ? Flags | CellFlags.Occupied : Flags & ~CellFlags.Occupied;
        }

        public void SetPath(bool path)
        {
            Flags = path ? Flags | CellFlags.Path : Flags & ~CellFlags.Path;
        }

        public void MoveTo(int newX, int newY)//todo spater <3
        {
            _packedCoord = (newX << 16) | (newY & 0xFFFF);
        }
    }
}