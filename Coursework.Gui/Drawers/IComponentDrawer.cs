using System.Windows.Controls;
using Coursework.Data;

namespace Coursework.Gui.Drawers
{
    public interface IComponentDrawer
    {
        void DrawComponents(Panel panel, INetwork network);
    }
}
