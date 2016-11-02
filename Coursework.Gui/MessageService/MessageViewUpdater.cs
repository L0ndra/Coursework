using System.Linq;
using System.Windows.Controls;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;

namespace Coursework.Gui.MessageService
{
    public class MessageViewUpdater : IMessageViewUpdater
    {
        private readonly IMessageRepository _messageRepository;
        private readonly TreeView _treeView;

        public MessageViewUpdater(IMessageRepository messageRepository, TreeView treeView)
        {
            _messageRepository = messageRepository;
            _treeView = treeView;
        }

        public void Show()
        {
            var messages = _messageRepository.GetAllMessages();

            RemoveAllChildren();

            foreach (var message in messages
                .Where(m => m != null)
                .OrderBy(m => m.ParentId))
            {
                _treeView.Items.Add(ConvertToTreeViewItem(message));
            }
        }

        private TreeViewItem ConvertToTreeViewItem(Message message)
        {
            var element = new TreeViewItem
            {
                Header = message.ParentId.ToString()
            };

            element.Items.Add($"Message type - {message.MessageType}");
            element.Items.Add($"Last transfer ID - {message.LastTransferNodeId}");
            element.Items.Add($"Receiver ID - {message.ReceiverId}");
            element.Items.Add($"Sender ID - {message.SenderId}");
            element.Items.Add($"Send Attempts - {message.SendAttempts}" );
            element.Items.Add($"Message Size - {message.Size}");

            return element;
        }

        private void RemoveAllChildren()
        {
            _treeView.Items.Clear();
        }
    }
}
