using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Coursework.Data;

namespace Coursework.Gui.Drawers
{
    public class NetworkDrawer : IComponentDrawer
    {
        private readonly IComponentDrawer _nodeDrawer;
        private readonly IComponentDrawer _channelDrawer;

        public NetworkDrawer(IComponentDrawer nodeDrawer, IComponentDrawer channelDrawer)
        {
            _nodeDrawer = nodeDrawer;
            _channelDrawer = channelDrawer;
        }

        public void DrawComponents(Panel panel, INetwork network)
        {
            var canvas = new Canvas()
            {
                Width = panel.ActualWidth,
                Height = panel.ActualHeight
            };

            _nodeDrawer.DrawComponents(canvas, network);
            _channelDrawer.DrawComponents(canvas, network);

            canvas.MouseUp += (object sender, MouseButtonEventArgs e) => RefreshChannels(sender as Panel, network);

            panel.Children.Add(canvas);
        }

        private void RefreshChannels(Panel panel, INetwork network)
        {
            var allLines = panel.Children
                .OfType<Line>()
                .ToArray();

            foreach (var line in allLines)
            {
                panel.Children.Remove(line);
            }

            _channelDrawer.DrawComponents(panel, network);
        }
    }
}
