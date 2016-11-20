using System;
using System.Windows;
using Coursework.Data.Services;
using Coursework.Gui.Helpers;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for NodeRemoveWindow.xaml
    /// </summary>
    public partial class NodeRemoveWindow
    {
        public delegate void NodeRemoveEventHandler(uint nodeId);
        private event NodeRemoveEventHandler NodeRemove;
        private readonly IExceptionDecorator _exceptionCatcher;

        public NodeRemoveWindow(NodeRemoveEventHandler nodeRemoveEventHandler)
        {
            InitializeComponent();

            NodeRemove += nodeRemoveEventHandler;

            _exceptionCatcher = new ExceptionCatcher();
        }

        private void RemoveNode_OnClick(object sender, RoutedEventArgs e)
        {
            Action action = () =>
            {
                var nodeId = uint.Parse(NodeId.Text);

                OnNodeRemove(nodeId);

                Close();
            };

            _exceptionCatcher.Decorate(action, ExceptionMessageBox.Show);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnNodeRemove(uint nodeid)
        {
            NodeRemove?.Invoke(nodeid);
        }
    }
}
