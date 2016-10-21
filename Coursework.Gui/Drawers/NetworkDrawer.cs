using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Coursework.Data;
using Coursework.Data.Constants;

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
                Height = panel.ActualHeight,
                Background = AllConstants.CanvasBrush
            };

            _nodeDrawer.DrawComponents(canvas, network);
            _channelDrawer.DrawComponents(canvas, network);

            canvas.MouseUp += (sender, e) => RefreshChannels(sender as Panel, network);

            panel.Children.Add(canvas);
        }

        private void RefreshChannels(Panel panel, INetwork network)
        {
            RemoveChannels(panel);

            _channelDrawer.DrawComponents(panel, network);
        }

        private static void RemoveChannels(Panel panel)
        {
            var elementsToClear = panel.Children
                .OfType<Line>()
                .Cast<UIElement>()
                .Concat(panel.Children.OfType<TextBlock>())
                .ToArray();


            foreach (var line in elementsToClear)
            {
                panel.Children.Remove(line);
            }
        }
    }
}
