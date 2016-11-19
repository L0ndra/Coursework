﻿using System;
using System.Collections.Immutable;
using System.Windows.Media;

namespace Coursework.Data.Constants
{
    public static class AllConstants
    {
        public const int NodeCountInMetropolitanNetwork = 8;
        public const int MetropolitanNetworksCount = 3;
        public const int MetropolitanNetworkNodesInRow = 3;
        public const double NetworkPower = 5.0;
        public const double TimerInterval = 500.0;
        public const double SquareSize = 30.0;
        public const double LineThickness = 2.0;
        public const double Eps = 0.002;
        public const double MessageGenerateChance = 0.1;
        public const int LineZIndex = int.MaxValue - 2;
        public const int PriceZIndex = int.MaxValue - 1;
        public const int MaxMessageSize = 2048;
        public const int PackageSize = 128;
        public const int ServicePartSize = 32;
        public const int InitializeMessageSize = 32;
        public const int SendingRequestMessageSize = 32;
        public const int SendingResponseMessageSize = 32;
        public static int UpdateTablePeriod = 40;
        public static readonly DoubleCollection StrokeDashArrayForSatteliteConnection = new DoubleCollection(new[]{ 2.0, 2.0 });
        public static readonly DoubleCollection StrokeDashArrayForGroundConnection = new DoubleCollection(new[] { 1.0, 0.0 });
        public static readonly Brush SimpleNodeBrush = Brushes.Aqua;
        public static readonly Brush CentralMachineBrush = Brushes.LightSeaGreen;
        public static readonly Brush MainMetropolitanMachineBrush = Brushes.DarkCyan;
        public static readonly Brush UnactiveNodeBrush = Brushes.Red;
        public static readonly Brush OutdatedNodeBrush = Brushes.Gray;
        public static readonly Brush ReceiverNodeBrush = Brushes.Green;
        public static readonly Brush CanvasBrush = Brushes.White;
        public static readonly Brush DuplexChannelBrush = Brushes.Black;
        public static readonly Brush HalfduplexChannelBrush = Brushes.DimGray;
        public static readonly Brush TwoMessagesInChannelBrush = Brushes.Blue;
        public static readonly Brush FirstMessageInChannelBrush = Brushes.Green;
        public static readonly Brush SecondMessageInChannelBrush = Brushes.DarkOrange;
        public static readonly Brush BusyChannelBrush = Brushes.DarkRed;
        public static readonly Brush ReceivedMessagesForeground = Brushes.Green;
        public static readonly Brush UnreceivedMessagesForeground = Brushes.Black;
        public static readonly Brush CanceledMessagesForeground = Brushes.Red;
        public static readonly Random RandomGenerator = new Random((int)(DateTime.Now.Ticks & 0xFFFF));
        public static readonly ImmutableArray<int> AllPrices = new[] { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 }.ToImmutableArray();
        public static readonly ImmutableArray<int> AllCapacities = new[] { 132, 120, 108, 96, 84, 72, 60, 48, 36, 24, 12}.ToImmutableArray();
    }
}