using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data;
using Coursework.Data.Builder;
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
        private readonly IComponentDrawer _networkDrawer;
        private readonly IComponentDrawer _nodeDrawer;
        private readonly IComponentDrawer _channelDrawer;

        public MainWindow(INetworkHandler network)
        {
            InitializeComponent();

            _network = network;

            _nodeDrawer = new NodeDrawer(_network);
            _channelDrawer = new ChannelDrawer(_network);
            _networkDrawer = new NetworkDrawer(_nodeDrawer, _channelDrawer);

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _networkDrawer.DrawComponents(NetworkArea);
        }

        private void AddNode_OnClick(object sender, RoutedEventArgs e)
        {
            var node = NodeGenerator.GenerateNodes(1).First();
            _network.AddNode(node);

            var generatedCanvas = NetworkArea.Children.OfType<Canvas>().First();

            _nodeDrawer.DrawComponents(generatedCanvas);
        }
    }
}
