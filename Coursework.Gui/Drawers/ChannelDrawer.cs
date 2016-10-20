using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Entities;
using Coursework.Gui.Dialogs;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class ChannelDrawer : IComponentDrawer
    {
        private static double SquareSize => 30.0;

        public void DrawComponents(Panel panel, INetwork network)
        {
            foreach (var channel in network.Channels)
            {
                var line = CreateLine(channel, network, panel);

                panel.Children.Add(line);
            }
        }

        private Line CreateLine(Channel channel, INetwork network, Panel panel)
        {
            var firstUiElement = GetElementByNodeId(network, panel, channel.FirstNodeId);
            var secondUiElement = GetElementByNodeId(network, panel, channel.SecondNodeId);

            var line = new Line
            {
                X1 = Canvas.GetLeft(firstUiElement) + SquareSize / 2,
                X2 = Canvas.GetLeft(secondUiElement) + SquareSize / 2,
                Y1 = Canvas.GetTop(firstUiElement) + SquareSize / 2,
                Y2 = Canvas.GetTop(secondUiElement) + SquareSize / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            line.Tag = Mapper.Map<Channel, ChannelDto>(channel);
            line.MouseUp += Line_MouseUp;
            line.Cursor = Cursors.Hand;

            return line;
        }

        private UIElement GetElementByNodeId(INetwork network, Panel panel, uint nodeId)
        {
            var node = network.Nodes.FirstOrDefault(n => n.Id == nodeId);
            var nodeIndex = Array.IndexOf(network.Nodes, node);
            var uiElement = panel.Children[nodeIndex];

            return uiElement;
        }

        private void Line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var line = sender as Line;

            var infoWindow = new ChannelInfoWindow();
            infoWindow.BindChannelInfo(line.Tag as ChannelDto);

            infoWindow.Show();
        }
    }
}
