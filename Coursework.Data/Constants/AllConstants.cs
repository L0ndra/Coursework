using System;
using System.Collections.Immutable;
using System.Windows.Media;

namespace Coursework.Data.Constants
{
    public static class AllConstants
    {
        public const uint NodeCount = 6;
        public const int MetropolitanNetworksCount = 4;
        public const double NetworkPower = 2.5;
        public const double SquareSize = 30.0;
        public const double LineThickness = 1.5;
        public const double Eps = 0.02;
        public static readonly DoubleCollection StrokeDashArrayForSatteliteConnection = new DoubleCollection(new[]{ 2.0, 2.0 });
        public static readonly DoubleCollection StrokeDashArrayForGroundConnection = new DoubleCollection(new[] { 1.0, 0.0 });
        public static readonly Brush NodeBrush = Brushes.Aqua;
        public static readonly Brush CanvasBrush = Brushes.Azure;
        public static readonly Brush DuplexChannelBrush = Brushes.Black;
        public static readonly Brush HalfduplexChannelBrush = Brushes.DimGray;
        public static readonly Random RandomGenerator = new Random((int)(DateTime.Now.Ticks & 0xFFFF));
        public static readonly ImmutableSortedSet<int> AllPrices = new[] { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 }.ToImmutableSortedSet();
    }
}