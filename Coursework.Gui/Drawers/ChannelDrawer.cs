using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Entities;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class ChannelDrawer : IComponentDrawer
    {
        private static double SquareSize => 30.0;

        public void DrawComponents(INetwork network, Panel panel)
        {
            foreach (var channel in network.Channels)
            {
                var firstUiElement = GetElementByNodeId(network, panel, channel.FirstNodeId);
                var secondUiElement = GetElementByNodeId(network, panel, channel.SecondNodeId);

                var line = CreateLine(Canvas.GetLeft(firstUiElement), Canvas.GetLeft(secondUiElement),
                    Canvas.GetTop(firstUiElement), Canvas.GetTop(secondUiElement));

                line.Tag = Mapper.Map<Channel, ChannelDto>(channel);

                panel.Children.Add(line);
            }
        }

        private Line CreateLine(double x1, double x2, double y1, double y2)
        {
            var line = new Line
            {
                X1 = x1 + SquareSize / 2,
                X2 = x2 + SquareSize / 2,
                Y1 = y1 + SquareSize / 2,
                Y2 = y2 + SquareSize / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            return line;
        }

        private UIElement GetElementByNodeId(INetwork network, Panel panel, uint nodeId)
        {
            var node = network.Nodes.FirstOrDefault(n => n.Id == nodeId);
            var nodeIndex = Array.IndexOf(network.Nodes, node);
            var uiElement = panel.Children[nodeIndex];

            return uiElement;
        }
    }
}
