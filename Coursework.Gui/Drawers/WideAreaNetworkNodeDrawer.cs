using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.Services;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class WideAreaNetworkNodeDrawer : NodeDrawer
    {
        private readonly IWideAreaNetworkService _wideAreaNetworkService;
        private Tuple<double, double>[] _groupsPosition;
        private Node[][] _groups;
        private const int ElementsInRow = 3;

        public WideAreaNetworkNodeDrawer(INetworkHandler network)
            : base(network)
        {
            _wideAreaNetworkService = new WideAreaNetworkService(network);
        }

        public override void DrawComponents(Panel panel)
        {
            _groups = _wideAreaNetworkService.GetNodesInOneMetropolitanNetwork();

            CountGroupsPositions(panel);

            base.DrawComponents(panel);
        }

        protected override Grid CreateGrid(FrameworkElement parent, NodeDto nodeDto, params UIElement[] childs)
        {
            var grid = base.CreateGrid(parent, nodeDto, childs);

            var currentNode = Network.GetNodeById(nodeDto.Id);
            var currentGroup = _groups.First(g => g.Any(n => n.Id == nodeDto.Id));
            var indexOfCurrentNodeInGroup = Array.IndexOf(currentGroup, currentNode);
            var groupNumber = Array.IndexOf(_groups, currentGroup);

            var columnNumber = indexOfCurrentNodeInGroup % ElementsInRow;
            var rowNumber = indexOfCurrentNodeInGroup / ElementsInRow;

            Canvas.SetLeft(grid, _groupsPosition[groupNumber].Item1
                + columnNumber * AllConstants.SquareSize);
            Canvas.SetTop(grid, _groupsPosition[groupNumber].Item2
                + rowNumber * AllConstants.SquareSize);

            return grid;
        }

        private void CountGroupsPositions(FrameworkElement parent)
        {
            var groupsCount = _groups.Length;
            _groupsPosition = new Tuple<double, double>[groupsCount];

            for (var i = 0; i < groupsCount; i++)
            {
                var groupSize = _groups[i].Length;

                var heightOfRows = AllConstants.SquareSize * (groupSize / ElementsInRow + 1);
                var widthOfColums = AllConstants.SquareSize * ElementsInRow;

                var startY = AllConstants.RandomGenerator.Next((int)(parent.Height - heightOfRows));

                var startX = AllConstants.RandomGenerator.Next((int)(parent.Width - widthOfColums));

                _groupsPosition[i] = new Tuple<double, double>(startX, startY);
            }
        }
    }
}
