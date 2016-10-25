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
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class NodeDrawer : IComponentDrawer
    {
        private readonly INetworkHandler _network;
        private readonly IList<Grid> _createdGrids = new List<Grid>();

        public NodeDrawer(INetworkHandler network)
        {
            _network = network;
        }

        public virtual void DrawComponents(Panel panel)
        {
            var createdNodes = _createdGrids
                .Where(uiElement => VisualTreeHelper.GetParent(uiElement).Equals(panel))
                .Select(uiElement => uiElement.Tag as NodeDto)
                .Select(n => n.Id);

            foreach (var node in _network.Nodes.Where(n => !createdNodes.Contains(n.Id)))
            {
                var rectangle = CreateRectangle();
                var textBlock = CreateTextBlock(node.Id.ToString());

                var grid = CreateGrid(panel, rectangle, textBlock);
                var nodeDto = Mapper.Map<Node, NodeDto>(node);
                grid.Tag = nodeDto;

                panel.Children.Add(grid);
                _createdGrids.Add(grid);
            }
        }

        public virtual void RemoveCreatedElements()
        {
            foreach (var uiElement in _createdGrids)
            {
                var parent = VisualTreeHelper.GetParent(uiElement) as Panel;
                parent?.Children.Remove(uiElement);
            }

            _createdGrids.Clear();
        }

        protected virtual Rectangle CreateRectangle()
        {
            var rectangle = new Rectangle
            {
                Fill = AllConstants.NodeBrush
            };

            rectangle.MouseMove += Rectangle_OnMouseMove;
            rectangle.MouseLeave += Rectangle_OnMouseLeave;

            return rectangle;
        }

        protected virtual TextBlock CreateTextBlock(string name)
        {
            var textBlock = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            return textBlock;
        }

        protected virtual Grid CreateGrid(FrameworkElement parent, params UIElement[] childs)
        {
            var grid = new Grid
            {
                Width = AllConstants.SquareSize,
                Height = AllConstants.SquareSize
            };

            foreach (var child in childs)
            {
                grid.Children.Add(child);
            }

            Canvas.SetTop(grid, AllConstants.RandomGenerator.Next((int)(parent.Height - grid.Height)));
            Canvas.SetLeft(grid, AllConstants.RandomGenerator.Next((int)(parent.Width - grid.Width)));

            return grid;
        }

        private static void Rectangle_OnMouseMove(object sender, MouseEventArgs e)
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

        private static void Rectangle_OnMouseLeave(object sender, MouseEventArgs e)
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
