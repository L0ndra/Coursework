using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Coursework.Data.Constants;

namespace Coursework.Gui.Drawers
{
    public class NetworkDrawer : IComponentDrawer
    {
        private readonly IComponentDrawer _nodeDrawer;
        private readonly IComponentDrawer _channelDrawer;
        private readonly IList<Canvas> _createdCanvases = new List<Canvas>();

        public NetworkDrawer(IComponentDrawer nodeDrawer, IComponentDrawer channelDrawer)
        {
            _nodeDrawer = nodeDrawer;
            _channelDrawer = channelDrawer;
        }

        public void DrawComponents(Panel panel)
        {
            var canvas = new Canvas()
            {
                Width = panel.ActualWidth,
                Height = panel.ActualHeight,
                Background = AllConstants.CanvasBrush
            };

            _nodeDrawer.DrawComponents(canvas);
            _channelDrawer.DrawComponents(canvas);

            canvas.MouseUp += (sender, e) => RefreshChannels(sender as Panel);

            panel.Children.Add(canvas);
            _createdCanvases.Add(canvas);
        }

        public void UpdateComponents()
        {
            _nodeDrawer.UpdateComponents();
            _channelDrawer.UpdateComponents();
        }

        public void RemoveCreatedElements()
        {
            foreach (var createdCanvas in _createdCanvases)
            {
                var parent = VisualTreeHelper.GetParent(createdCanvas) as Panel;
                parent?.Children.Remove(createdCanvas);
            }

            _createdCanvases.Clear();
        }

        private void RefreshChannels(Panel panel)
        {
            _channelDrawer.RemoveCreatedElements();

            _channelDrawer.DrawComponents(panel);
        }
    }
}
