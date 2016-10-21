using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Gui.Dialogs;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class ChannelDrawer : IComponentDrawer
    {
        private readonly INetworkHandler _network;
        private readonly IList<UIElement> _uiElements = new List<UIElement>();

        public ChannelDrawer(INetworkHandler network)
        {
            _network = network;
        }

        public void DrawComponents(Panel panel)
        {
            foreach (var channel in _network.Channels)
            {
                var line = CreateLine(channel, panel);
                var textBlock = CreateTextBlock(channel, line);

                panel.Children.Add(line);
                panel.Children.Add(textBlock);

                _uiElements.Add(line);
                _uiElements.Add(textBlock);
            }
        }

        public void RemoveCreatedElements()
        {
            foreach (var uiElement in _uiElements)
            {
                var parent = VisualTreeHelper.GetParent(uiElement) as Panel;
                parent?.Children.Remove(uiElement);
            }

            _uiElements.Clear();
        }

        private Line CreateLine(Channel channel, Panel panel)
        {
            var firstUiElement = GetElementByNodeId(panel, channel.FirstNodeId);
            var secondUiElement = GetElementByNodeId(panel, channel.SecondNodeId);

            var line = new Line
            {
                X1 = Canvas.GetLeft(firstUiElement) + AllConstants.SquareSize / 2,
                X2 = Canvas.GetLeft(secondUiElement) + AllConstants.SquareSize / 2,
                Y1 = Canvas.GetTop(firstUiElement) + AllConstants.SquareSize / 2,
                Y2 = Canvas.GetTop(secondUiElement) + AllConstants.SquareSize / 2,
                Stroke = AllConstants.LineBrush,
                StrokeThickness = AllConstants.LineThickness,
                StrokeDashArray = GetStrokeDashArray(channel),
                Tag = Mapper.Map<Channel, ChannelDto>(channel),
                Cursor = Cursors.Hand
            };

            line.MouseUp += Line_MouseUp;

            return line;
        }

        private static DoubleCollection GetStrokeDashArray(Channel channel)
        {
            return channel.ChannelType == ChannelType.Ground
                ? AllConstants.StrokeDashArrayForGroundConnection
                : AllConstants.StrokeDashArrayForSatteliteConnection;
        }

        private TextBlock CreateTextBlock(Channel channel, Line connectedLine)
        {
            var textBlock = new TextBlock
            {
                Text = channel.Price.ToString("N"),
                Background = AllConstants.CanvasBrush
            };

            Canvas.SetTop(textBlock, (connectedLine.Y1 + connectedLine.Y2) / 2);
            Canvas.SetLeft(textBlock, (connectedLine.X1 + connectedLine.X2) / 2);

            return textBlock;
        }

        private UIElement GetElementByNodeId(Panel panel, uint nodeId)
        {
            var node = _network.Nodes.FirstOrDefault(n => n.Id == nodeId);
            var nodeIndex = Array.IndexOf(_network.Nodes, node);
            var uiElement = panel.Children[nodeIndex];

            return uiElement;
        }

        private void Line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var line = sender as Line;

            var infoWindow = new ChannelInfoWindow();
            if (line != null)
            {
                infoWindow.BindChannelInfo(line.Tag as ChannelDto, UpdateChannels);
            }

            infoWindow.Show();
        }

        private void UpdateChannels(ChannelDto channelDto)
        {
            var newChannel = Mapper.Map<ChannelDto, Channel>(channelDto);
            _network.UpdateChannel(newChannel);

            var parents = _uiElements
                .Select(uiElement => VisualTreeHelper.GetParent(_uiElements.First()) as Panel)
                .GroupBy(panel => panel)
                .Select(g => g.Key)
                .ToArray();

            RemoveCreatedElements();
            foreach (var parent in parents)
            {
                DrawComponents(parent);
            }
        }
    }
}
