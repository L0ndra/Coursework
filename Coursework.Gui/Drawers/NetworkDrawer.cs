using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Coursework.Data;

namespace Coursework.Gui.Drawers
{
    public class NetworkDrawer : INetworkDrawer
    {
        private readonly IComponentDrawer _nodeDrawer;
        private readonly IComponentDrawer _channelDrawer;

        public NetworkDrawer(IComponentDrawer nodeDrawer, IComponentDrawer channelDrawer)
        {
            _nodeDrawer = nodeDrawer;
            _channelDrawer = channelDrawer;
        }

        public Panel DrawNetwork(INetwork network, double width, double height)
        {
            var canvas = new Canvas()
            {
                Width = width,
                Height = height
            };

            _nodeDrawer.DrawComponents(network, canvas);
            _channelDrawer.DrawComponents(network, canvas);

            canvas.MouseUp += (object sender, MouseButtonEventArgs e) => RefreshChannels(network, sender as Panel);

            return canvas;
        }

        private void RefreshChannels(INetwork network, Panel panel)
        {
            var allLines = panel.Children
                .OfType<Line>()
                .ToArray();

            foreach (var line in allLines)
            {
                panel.Children.Remove(line);
            }

            _channelDrawer.DrawComponents(network, panel);
        }
    }
}
