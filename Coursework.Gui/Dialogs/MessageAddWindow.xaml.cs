using System;
using System.Windows;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageAddWindow.xaml
    /// </summary>
    public partial class MessageAddWindow
    {
        public delegate void MessageCreateHandler(MessageInitializer messageInitializer);
        private event MessageCreateHandler MessageCreateEvent;

        public MessageAddWindow(MessageCreateHandler messageCreateHandler)
        {
            InitializeComponent();

            MessageCreateEvent += messageCreateHandler;
        }

        private void CreateMessage_OnClick(object sender, RoutedEventArgs e)
        {
            try
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
            }
            catch (Exception ex) when (ex is ChannelException || ex is NodeException ||
                ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
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
