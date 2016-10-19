using System.Windows.Controls;

namespace Coursework.Data.Drawers
{
    public interface IComponentDrawer
    {
        void DrawComponents(INetwork network, Panel panel);
    }
}
