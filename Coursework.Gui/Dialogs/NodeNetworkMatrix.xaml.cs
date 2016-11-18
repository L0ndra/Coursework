using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Coursework.Data.NetworkData;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for NodeNetworkMatrix.xaml
    /// </summary>
    public partial class NodeNetworkMatrix
    {
        private const double Size = 50.0;

        public NodeNetworkMatrix(INetwork network, uint nodeId)
        {
            InitializeComponent();

            CreateRowsAndColumns(network);

            CreateHeaders(network);

            FillMatrix(network, nodeId);

            Title = $"Network matrix (node {nodeId})";
        }

        private void FillMatrix(INetwork network, uint nodeId)
        {
            var networkMatrix = network.Nodes
                .First(n => n.Id == nodeId)
                .NetworkMatrix
                .PriceMatrix;

            for (var i = 0; i < network.Nodes.Length; i++)
            {
                var rowNode = network.Nodes[i];

                for (var j = 0; j < network.Nodes.Length; j++)
                {
                    var columnNode = network.Nodes[j];

                    var border = CreateBorder(i, j);

                    Table.Children.Add(border);

                    Table.Children.Add(CreateText(networkMatrix[rowNode.Id][columnNode.Id].ToString("N", CultureInfo.InvariantCulture),
                        i + 1, j + 1));
                }
            }
        }

        private static Border CreateBorder(int i, int j)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            Grid.SetRow(border, i + 1);
            Grid.SetColumn(border, j + 1);

            return border;
        }

        private void CreateHeaders(INetwork network)
        {
            for (var i = 0; i < network.Nodes.Length; i++)
            {
                var nodeId = network.Nodes[i].Id;

                Table.Children.Add(CreateText(nodeId.ToString(), 0, i + 1));
                Table.Children.Add(CreateText(nodeId.ToString(), i + 1, 0));
            }
        }

        private void CreateRowsAndColumns(INetwork network)
        {
            for (var i = 0; i < network.Nodes.Length + 1; i++)
            {
                var rowDefinition = new RowDefinition();
                var columnDefinition = new ColumnDefinition();

                rowDefinition.Height = new GridLength(Size);
                columnDefinition.Width = new GridLength(Size);

                Table.RowDefinitions.Add(rowDefinition);
                Table.ColumnDefinitions.Add(columnDefinition);
            }
        }

        private TextBlock CreateText(string text, int row, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = Size,
            };

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);

            return textBlock;
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
