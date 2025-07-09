using System.Runtime.CompilerServices;

namespace TowerDefense.Core
{
    public static class CoordPacker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Pack(int x, int y) => (x << 16) | (y & 0xFFFF);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UnpackX(int packed) => packed >> 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UnpackY(int packed) => (short)(packed & 0xFFFF);
    }
}