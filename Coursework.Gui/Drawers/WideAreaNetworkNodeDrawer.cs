using System;
using System.Windows.Controls;
using Coursework.Data;

namespace Coursework.Gui.Drawers
{
    public class WideAreaNetworkNodeDrawer : NodeDrawer
    {
        public WideAreaNetworkNodeDrawer(INetworkHandler network) 
            : base(network)
        {
        }

        public override void DrawComponents(Panel panel)
        {
            throw new NotImplementedException();
        }

        public override void RemoveCreatedElements()
        {
            throw new NotImplementedException();
        }
    }
}
