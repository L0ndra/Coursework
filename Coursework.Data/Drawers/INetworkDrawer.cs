using System.Windows.Controls;

namespace Coursework.Data.Drawers
{
    public interface INetworkDrawer
    {
        Panel DrawNetwork(INetwork network, double width, double height);
    }
}
