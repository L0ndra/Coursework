using System;
using System.Windows;
using Coursework.Data.Entities;
using Coursework.Data.Services;
using Coursework.Gui.Helpers;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageAddWindow.xaml
    /// </summary>
    public partial class MessageAddWindow
    {
        public delegate void MessageCreateHandler(MessageInitializer messageInitializer);
        private event MessageCreateHandler MessageCreateEvent;
        private readonly IExceptionDecorator _exceptionCatcher;

        public MessageAddWindow(MessageCreateHandler messageCreateHandler)
        {
            InitializeComponent();

            MessageCreateEvent += messageCreateHandler;

            _exceptionCatcher = new ExceptionCatcher();
        }

        private void CreateMessage_OnClick(object sender, RoutedEventArgs e)
        {
            Action action = () =>
            {
                var senderId = uint.Parse(SenderId.Text);
                var receiverId = uint.Parse(ReceiverId.Text);
                var size = int.Parse(Size.Text);

                var messageInitializer = new MessageInitializer
                {
                    MessageType = MessageType.General,
                    ReceiverId = receiverId,
                    SenderId = senderId,
                    Data = null,
                    Size = size
                };

                OnMessageCreateEvent(messageInitializer);

                MessageBox.Show("Message Created", "Ok", MessageBoxButton.OK, MessageBoxImage.Information,
                   MessageBoxResult.OK,
                   MessageBoxOptions.None);
            };

            _exceptionCatcher.Decorate(action, ExceptionMessageBox.Show);
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnMessageCreateEvent(MessageInitializer messageinitializer)
        {
            MessageCreateEvent?.Invoke(messageinitializer);
        }
    }
}
