using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class NodeDrawer : IComponentDrawer
    {
        protected readonly INetworkHandler Network;
        protected readonly IList<Grid> CreatedGrids = new List<Grid>();

        public NodeDrawer(INetworkHandler network)
        {
            Network = network;
        }

        public virtual void DrawComponents(Panel panel)
        {
            var createdNodes = CreatedGrids
                .Where(uiElement => VisualTreeHelper.GetParent(uiElement).Equals(panel))
                .Select(uiElement => uiElement.Tag as NodeDto)
                .Select(n => n.Id);

            foreach (var node in Network.Nodes.Where(n => !createdNodes.Contains(n.Id)))
            {
                var textBlock = CreateTextBlock(node.Id.ToString());
                
                var nodeDto = Mapper.Map<Node, NodeDto>(node);
                var grid = CreateGrid(panel, nodeDto, textBlock);

                panel.Children.Add(grid);
                CreatedGrids.Add(grid);
            }
        }

        public virtual void RemoveCreatedElements()
        {
            foreach (var uiElement in CreatedGrids)
            {
                var parent = VisualTreeHelper.GetParent(uiElement) as Panel;
                parent?.Children.Remove(uiElement);
            }

            CreatedGrids.Clear();
        }

        protected virtual TextBlock CreateTextBlock(string name)
        {
            var textBlock = new TextBlock
            {
                Text = name,
                Width = AllConstants.SquareSize,
                Height = AllConstants.SquareSize,
                TextAlignment = TextAlignment.Center
            };

            textBlock.MouseMove += Node_OnMouseMove;
            textBlock.MouseLeave += Node_OnMouseLeave;

            return textBlock;
        }

        protected virtual Grid CreateGrid(FrameworkElement parent, NodeDto nodeDto, params UIElement[] childs)
        {
            var grid = new Grid
            {
                Width = AllConstants.SquareSize,
                Height = AllConstants.SquareSize,
                Background = nodeDto.NodeType == NodeType.SimpleNode
                    ? AllConstants.SimpleNodeBrush
                    : AllConstants.CentralMachineBrush,
                Tag = nodeDto
            };

            foreach (var child in childs)
            {
                grid.Children.Add(child);
            }

            Canvas.SetTop(grid, AllConstants.RandomGenerator.Next((int)(parent.Height - grid.Height)));
            Canvas.SetLeft(grid, AllConstants.RandomGenerator.Next((int)(parent.Width - grid.Width)));

            return grid;
        }

        private static void Node_OnMouseMove(object sender, MouseEventArgs e)
        {
            var concreteSender = (FrameworkElement)sender;
            var parent = (FrameworkElement)concreteSender?.Parent;

            if (parent != null)
            {
                Panel.SetZIndex(parent, int.MaxValue);

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var mainParent = parent.Parent as IInputElement;

                    var currentYPosition = e.GetPosition(mainParent).Y - parent.Height / 2;
                    var currentXPosition = e.GetPosition(mainParent).X - parent.Width / 2;

                    Canvas.SetTop(parent, currentYPosition);
                    Canvas.SetLeft(parent, currentXPosition);
                }
            }
        }

        private static void Node_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var concreteSender = sender as FrameworkElement;
            var parent = concreteSender?.Parent as FrameworkElement;

            if (parent != null)
            {
                Panel.SetZIndex(parent, int.MinValue);
            }
        }
    }
}
