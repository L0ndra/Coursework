using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;

namespace Coursework.Gui.MessageService
{
    public class MessageViewUpdater : IMessageViewUpdater
    {
        public MessageFiltrationMode MessageFiltrationMode { get; set; } = MessageFiltrationMode.ActiveMessages;
        private readonly IMessageRepository _messageRepository;
        private readonly TreeView _treeView;

        public MessageViewUpdater(IMessageRepository messageRepository, TreeView treeView)
        {
            _messageRepository = messageRepository;
            _treeView = treeView;
        }

        public void Show()
        {
            var messages = _messageRepository.GetAllMessages(messageFiltrationMode: MessageFiltrationMode);

            RemoveAllChildren();

            var groupedMessages = messages
                .GroupBy(m => m.ParentId)
                .OrderBy(m => m.Select(m1 => m1.IsReceived).Aggregate((b, b1) => b && b1) ||
                    m.Select(m1 => m1.IsCanceled).Aggregate((b, b1) => b && b1))
                .ThenBy(m => m.Key)
                .Select(g => new
                {
                    ParentId = g.Key,
                    Messages = g
                        .OrderBy(m => m.IsReceived)
                        .ThenBy(m => m.NumberInPackage)
                });

            foreach (var groupOfMessages in groupedMessages)
            {
                _treeView.Items.Add(ConvertToTreeViewItem(groupOfMessages.Messages.ToArray()));
            }
        }

        private TreeViewItem ConvertToTreeViewItem(Message[] messages)
        {
            var header = messages.First().ParentId;

            var element = new TreeViewItem
            {
                Header = header,
                Foreground = GetForeground(messages)
            };

            foreach (var message in messages)
            {
                element.Items.Add(ConvertToTreeViewItem(message));
            }

            element.MouseLeftButtonUp += Element_OnMouseDoubleClick;

            return element;
        }

        private void Element_OnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var treeViewItem = (TreeViewItem)sender;

            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;

            foreach (var treeViewInnerItem in treeViewItem.Items.OfType<TreeViewItem>())
            {
                Element_OnMouseDoubleClick(treeViewInnerItem, mouseButtonEventArgs);
            }
        }

        private Brush GetForeground(IEnumerable<Message> messages)
        {
            var messagesArray = messages as Message[] ?? messages.ToArray();

            if (messagesArray.All(m => m.IsReceived))
            {
                return AllConstants.ReceivedMessagesForeground;
            }

            if (messagesArray.All(m => m.IsCanceled))
            {
                return AllConstants.CanceledMessagesForeground;
            }

            return AllConstants.UnreceivedMessagesForeground;
        }

        private TreeViewItem ConvertToTreeViewItem(Message message)
        {
            var foreground = GetForeground(new[] { message });

            var element = new TreeViewItem
            {
                Header = $"Message Number - {message.NumberInPackage}",
                Foreground = foreground
            };

            AddBlocks(message, element);

            var routeElement = CreateRoute(message);

            element.Items.Add(routeElement);

            return element;
        }

        private void AddBlocks(Message message, ItemsControl element)
        {
            var foreground = GetForeground(new[] { message });

            element.Items.Add(CreateNestedTreeViewItem($"Message type - {message.MessageType}",
                foreground));

            element.Items.Add(CreateNestedTreeViewItem($"Last transfer ID - {message.LastTransferNodeId}",
                foreground));

            element.Items.Add(CreateNestedTreeViewItem($"Receiver ID - {message.ReceiverId}",
                foreground));

            element.Items.Add(CreateNestedTreeViewItem($"Sender ID - {message.SenderId}",
                foreground));

            element.Items.Add(CreateNestedTreeViewItem($"Send Attempts - {message.SendAttempts}",
                foreground));

            element.Items.Add(CreateNestedTreeViewItem($"Message Size - {message.Size}",
                foreground));
        }

        private TextBlock CreateNestedTreeViewItem(string text, Brush foreground)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = foreground
            };
        }

        private TreeViewItem CreateRoute(Message message)
        {
            var foreground = GetForeground(new[] { message });

            var routeElement = new TreeViewItem
            {
                Header = "Route",
                Foreground = foreground
            };

            var startId = message.LastTransferNodeId;

            foreach (var channel in message.Route)
            {
                if (channel.FirstNodeId == startId)
                {
                    routeElement.Items.Add(CreateNestedTreeViewItem($"From {channel.FirstNodeId} to {channel.SecondNodeId} " +
                                           $"({channel.Price}/{channel.ErrorChance:N})", foreground));

                    startId = channel.SecondNodeId;
                }
                else
                {
                    routeElement.Items.Add(CreateNestedTreeViewItem($"From {channel.SecondNodeId} to {channel.FirstNodeId} " +
                                           $"({channel.Price}/{channel.ErrorChance:N})", foreground));

                    startId = channel.FirstNodeId;
                }
            }

            return routeElement;
        }

        private void RemoveAllChildren()
        {
            _treeView.Items.Clear();
        }
    }
}
