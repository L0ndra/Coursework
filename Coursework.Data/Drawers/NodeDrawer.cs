using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Coursework.Data.Drawers
{
    public class NodeDrawer : IComponentDrawer
    {
        private readonly Random _randomGenerator;
        private static double SquareSize => 30.0;
        private static Brush BrushColor => Brushes.Aqua;

        public NodeDrawer(Random randomGenerator)
        {
            _randomGenerator = randomGenerator;
        }

        public void DrawComponents(INetwork network, Panel panel)
        {
            foreach (var node in network.Nodes)
            {
                var rectangle = CreateRectangle();
                var textBlock = CreateTextBlock(node.Id.ToString());

                var grid = CreateGrid(panel, rectangle, textBlock);

                panel.Children.Add(grid);
            }
        }

        private Rectangle CreateRectangle()
        {
            var rectangle = new Rectangle
            {
                Fill = BrushColor
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
                Width = SquareSize,
                Height = SquareSize
            };

            foreach (var child in childs)
            {
                grid.Children.Add(child);
            }

            Canvas.SetTop(grid, _randomGenerator.Next((int)(parent.Height - grid.Height)));
            Canvas.SetLeft(grid, _randomGenerator.Next((int)(parent.Width - grid.Width)));

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
