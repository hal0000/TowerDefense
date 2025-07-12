using System.Runtime.CompilerServices;

namespace TowerDefense.Core
{
    public static class CoordPacker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Pack(int x, int y)
        {
            return (x << 16) | (y & 0xFFFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UnpackX(int packed)
        {
            return packed >> 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UnpackY(int packed)
        {
            return (short)(packed & 0xFFFF);
        }
    }
}