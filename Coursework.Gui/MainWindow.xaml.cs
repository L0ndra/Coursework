using System.Windows;
using Coursework.Data;
using Coursework.Gui.Drawers;
using MahApps.Metro.Controls;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly INetworkHandler _network;

        public MainWindow(INetworkHandler network)
        {
            InitializeComponent();

            _network = network;

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = NetworkArea;

            var nodeDrawer = new NodeDrawer(_network);
            var channelDrawer = new ChannelDrawer(_network);
            var networkDrawer = new NetworkDrawer(nodeDrawer, channelDrawer);

            networkDrawer.DrawComponents(frameworkElement);
        }
    }
}
