using System;
using System.Windows;
using Coursework.Data.Exceptions;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ChannelRemoveWindow.xaml
    /// </summary>
    public partial class ChannelRemoveWindow
    {
        public delegate void ChannelRemoveEventHandler(uint firstNodeId, uint secondNodeId);
        private event ChannelRemoveEventHandler ChannelRemove;

        public ChannelRemoveWindow(ChannelRemoveEventHandler channelRemoveEventHandler)
        {
            InitializeComponent();

            ChannelRemove += channelRemoveEventHandler;
        }

        private void RemoveChannel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var firstNodeId = uint.Parse(FirstNodeId.Text);
                var secondNodeId = uint.Parse(SecondNodeId.Text);

                OnChannelRemove(firstNodeId, secondNodeId);

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

        protected virtual void OnChannelRemove(uint firstNodeId, uint secondNodeId)
        {
            ChannelRemove?.Invoke(firstNodeId, secondNodeId);
        }
    }
}
