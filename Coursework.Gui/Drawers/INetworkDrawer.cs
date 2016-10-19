using System.Windows.Controls;
using Coursework.Data;

namespace Coursework.Gui.Drawers
{
    public interface INetworkDrawer
    {
        Panel DrawNetwork(INetwork network, double width, double height);
    }
}
