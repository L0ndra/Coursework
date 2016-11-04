using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data.IONetwork;
using Coursework.Data.NetworkData;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Drawers
{
    public class SmartNodeDrawer : NodeDrawer
    {
        private readonly NodeLocationDto[] _nodeLocations;

        public SmartNodeDrawer(INetworkHandler network, NodeLocationDto[] nodeLocations) : base(network)
        {
            _nodeLocations = nodeLocations;
        }

        protected override Grid CreateGrid(FrameworkElement parent, NodeDto nodeDto, params UIElement[] childs)
        {
            var grid = base.CreateGrid(parent, nodeDto, childs);

            var location = _nodeLocations.FirstOrDefault(n => n.Id == nodeDto.Id);

            if (location != null)
            {
                Canvas.SetTop(grid, location.Y);
                Canvas.SetLeft(grid, location.X);
            }

            return grid;
        }
    }
}
