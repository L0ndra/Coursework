using System;
using System.Collections.Immutable;
using System.Windows.Media;

namespace Coursework.Data.Constants
{
    public static class AllConstants
    {
        public const uint NodeCount = 10;
        public const double NetworkPower = 2.0;
        public static double SquareSize = 30.0;
        public static Brush NodeBrush => Brushes.Aqua;
        public static Random RandomGenerator = new Random((int)(DateTime.Now.Ticks & 0xFFFF));
        public static ImmutableSortedSet<int> AllPrices = new[] { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 }.ToImmutableSortedSet();
    }
}
