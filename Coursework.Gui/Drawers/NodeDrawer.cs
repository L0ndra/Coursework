using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoMapper;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;
using Coursework.Gui.Dialogs;
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

        public virtual void UpdateComponents()
        {
            var elementsToRemove = new List<Grid>();

            foreach (var createdGrid in CreatedGrids)
            {
                var nodeDto = (NodeDto)createdGrid.Tag;
                var node = Network.GetNodeById(nodeDto.Id);

                if (node == null)
                {
                    elementsToRemove.Add(createdGrid);
                }
                else
                {
                    createdGrid.Tag = Mapper.Map<Node, NodeDto>(node);
                    nodeDto = (NodeDto)createdGrid.Tag;

                    createdGrid.Background = GetBackground(nodeDto);
                }
            }

            RemoveRange(elementsToRemove);
        }

        public virtual void RemoveCreatedElements()
        {
            RemoveRange(CreatedGrids.ToArray());
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
            textBlock.MouseRightButtonUp += Node_OnMouseRightButtonUp;
            textBlock.MouseDown += Node_OnMouseDown;

            return textBlock;
        }

        private void Node_OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var concreteSender = (FrameworkElement)sender;
            var parent = (FrameworkElement)concreteSender?.Parent;

            if (parent != null)
            {
                var nodeDto = (NodeDto)parent.Tag;

                if (mouseButtonEventArgs.ClickCount == 2)
                {
                    var nodeNetwrokMatrix = new NodeNetworkMatrix(Network, nodeDto.Id);

                    nodeNetwrokMatrix.Show();
                }
            }
        }

        protected virtual Grid CreateGrid(FrameworkElement parent, NodeDto nodeDto, params UIElement[] childs)
        {
            var grid = new Grid
            {
                Width = AllConstants.SquareSize,
                Height = AllConstants.SquareSize,
                Background = GetBackground(nodeDto),
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

        private static Brush GetBackground(NodeDto nodeDto)
        {
            if (nodeDto.GotReceivedMessages)
            {
                return AllConstants.ReceiverNodeBrush;
            }
            if (!nodeDto.IsActive)
            {
                return AllConstants.UnactiveNodeBrush;
            }
            if (!nodeDto.IsTableUpdated)
            {
                return AllConstants.OutdatedNodeBrush;
            }
            switch (nodeDto.NodeType)
            {
                case NodeType.SimpleNode:
                    {
                        return AllConstants.SimpleNodeBrush;
                    }
                case NodeType.MainMetropolitanMachine:
                    {
                        return AllConstants.MainMetropolitanMachineBrush;
                    }
                case NodeType.CentralMachine:
                    {
                        return AllConstants.CentralMachineBrush;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(nodeDto));
                    }
            }
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

        private void Node_OnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var concreteSender = (FrameworkElement)sender;
            var parent = (FrameworkElement)concreteSender?.Parent;

            if (parent != null)
            {
                var nodeDto = (NodeDto)parent.Tag;

                var node = Network.GetNodeById(nodeDto.Id);

                node.IsActive = !node.IsActive;
            }

            UpdateComponents();
        }

        private void RemoveRange(IEnumerable<Grid> rangeToRemove)
        {
            foreach (var element in rangeToRemove)
            {
                var parent = VisualTreeHelper.GetParent(element) as Panel;
                parent?.Children.Remove(element);

                CreatedGrids.Remove(element);
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
