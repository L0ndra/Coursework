using System;
using System.Windows;
using System.Windows.Input;
using Coursework.Data;
using Coursework.Data.Drawers;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly INetwork _network;
        private readonly INetworkDrawer _networkDrawer;

        public MainWindow(INetwork network, INetworkDrawer networkDrawer)
        {
            InitializeComponent();

            _network = network;
            _networkDrawer = networkDrawer;

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = NetworkArea;

            var canvas = _networkDrawer.DrawNetwork(_network, frameworkElement.ActualWidth,
                frameworkElement.ActualHeight);

            NetworkArea.Children.Add(canvas);
        }
    }
}
