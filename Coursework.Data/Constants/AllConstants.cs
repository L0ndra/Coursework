using System;
using System.Collections.Immutable;
using System.Windows.Media;

namespace Coursework.Data.Constants
{
    public static class AllConstants
    {
        public const uint NodeCountInMetropolitanNetwork = 8;
        public const int MetropolitanNetworksCount = 3;
        public const int MetropolitanNetworkNodesInRow = 3;
        public const double NetworkPower = 5.0;
        public const double TimerInterval = 500.0;
        public const double SquareSize = 30.0;
        public const double LineThickness = 2.0;
        public const double Eps = 0.002;
        public const double MessageGenerateChance = 0.2;
        public const int InitializeMessageSize = 5;
        public const int LineZIndex = int.MaxValue - 2;
        public const int PriceZIndex = int.MaxValue - 1;
        public const int MaxAttempts = 100;
        public const int MaxAttemptsToGenerateMessage = 50;
        public const int MaxMessageSize = 100;
        public const int PackageSize = 10;
        public const int ServicePartSize = 1;
        public const int SendingRequestMessageSize = 10;
        public const int SendingResponseMessageSize = 10;
        public static int UpdateTablePeriod = 40;
        public static readonly DoubleCollection StrokeDashArrayForSatteliteConnection = new DoubleCollection(new[]{ 2.0, 2.0 });
        public static readonly DoubleCollection StrokeDashArrayForGroundConnection = new DoubleCollection(new[] { 1.0, 0.0 });
        public static readonly Brush SimpleNodeBrush = Brushes.Aqua;
        public static readonly Brush CentralMachineBrush = Brushes.LightSeaGreen;
        public static readonly Brush MainMetropolitanMachineBrush = Brushes.DarkCyan;
        public static readonly Brush UnactiveNodeBrush = Brushes.Red;
        public static readonly Brush OutdatedNodeBrush = Brushes.Gray;
        public static readonly Brush ReceiverNodeBrush = Brushes.Green;
        public static readonly Brush CanvasBrush = Brushes.Azure;
        public static readonly Brush DuplexChannelBrush = Brushes.Black;
        public static readonly Brush HalfduplexChannelBrush = Brushes.DimGray;
        public static readonly Brush TwoMessagesInChannelBrush = Brushes.Blue;
        public static readonly Brush FirstMessageInChannelBrush = Brushes.Green;
        public static readonly Brush SecondMessageInChannelBrush = Brushes.DarkOrange;
        public static readonly Brush ReceivedMessagesForeground = Brushes.Green;
        public static readonly Brush UnreceivedMessagesForeground = Brushes.Black;
        public static readonly Brush CanceledMessagesForeground = Brushes.Red;
        public static readonly Random RandomGenerator = new Random((int)(DateTime.Now.Ticks & 0xFFFF));
        public static readonly ImmutableSortedSet<int> AllPrices = new[] { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 }.ToImmutableSortedSet();
    }
}