using System;
using System.Windows;
using Coursework.Data.Exceptions;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for NodeRemoveWindow.xaml
    /// </summary>
    public partial class NodeRemoveWindow
    {
        public delegate void NodeRemoveEventHandler(uint nodeId);
        private event NodeRemoveEventHandler NodeRemove;

        public NodeRemoveWindow(NodeRemoveEventHandler nodeRemoveEventHandler)
        {
            InitializeComponent();

            NodeRemove += nodeRemoveEventHandler;
        }

        private void RemoveNode_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var nodeId = uint.Parse(NodeId.Text);

                OnNodeRemove(nodeId);

                Close();
            }
            catch (Exception ex) when (ex is ChannelException || ex is NodeException ||
                ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
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
