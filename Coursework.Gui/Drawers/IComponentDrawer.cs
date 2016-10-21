using System.Windows.Controls;

namespace Coursework.Gui.Drawers
{
    public interface IComponentDrawer
    {
        void DrawComponents(Panel panel);
        void RemoveCreatedElements();
    }
}
