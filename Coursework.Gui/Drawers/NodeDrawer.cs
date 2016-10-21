using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public void DrawComponents(Panel panel, INetwork network)
        {
            foreach (var node in network.Nodes)
            {
                var rectangle = CreateRectangle();
                var textBlock = CreateTextBlock(node.Id.ToString());

                var grid = CreateGrid(panel, rectangle, textBlock);
                grid.Tag = Mapper.Map<Node, NodeDto>(node);

                panel.Children.Add(grid);
            }
        }

        private Rectangle CreateRectangle()
        {
            var rectangle = new Rectangle
            {
                Fill = AllConstants.NodeBrush
            };

            rectangle.MouseMove += Rectangle_OnMouseMove;
            rectangle.MouseLeave += Rectangle_OnMouseLeave;

            return rectangle;
        }

        private TextBlock CreateTextBlock(string name)
        {
            var textBlock = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            return textBlock;
        }

        private Grid CreateGrid(FrameworkElement parent, params UIElement[] childs)
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
            var concreteSender = sender as FrameworkElement;
            var parent = concreteSender?.Parent as FrameworkElement;

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
