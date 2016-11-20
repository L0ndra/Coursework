using System;
using System.Windows;
using Coursework.Data.Services;
using Coursework.Gui.Helpers;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ChannelRemoveWindow.xaml
    /// </summary>
    public partial class ChannelRemoveWindow
    {
        public delegate void ChannelRemoveEventHandler(uint firstNodeId, uint secondNodeId);
        private event ChannelRemoveEventHandler ChannelRemove;
        private readonly IExceptionDecorator _exceptionCatcher;

        public ChannelRemoveWindow(ChannelRemoveEventHandler channelRemoveEventHandler)
        {
            InitializeComponent();

            ChannelRemove += channelRemoveEventHandler;

            _exceptionCatcher = new ExceptionCatcher();
        }

        private void RemoveChannel_OnClick(object sender, RoutedEventArgs e)
        {
            Action action = () =>
            {
                var firstNodeId = uint.Parse(FirstNodeId.Text);
                var secondNodeId = uint.Parse(SecondNodeId.Text);

                OnChannelRemove(firstNodeId, secondNodeId);

                Close();
            };

            _exceptionCatcher.Decorate(action, ExceptionMessageBox.Show);
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
