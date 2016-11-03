using System;
using System.Windows.Controls;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Coursework.Gui.MessageService;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageViewUpdaterTests
    {
        private Mock<IMessageRepository> _messageRepoMock;
        private IMessageViewUpdater _messageViewUpdater;
        private Message[] _messages;
        private TreeView _treeView;

        [SetUp]
        public void Setup()
        {
            _messageRepoMock = new Mock<IMessageRepository>();
            _treeView = new TreeView();

            _messageViewUpdater = new MessageViewUpdater(_messageRepoMock.Object, _treeView);

            _messages = new[]
            {
                new Message
                {
                    MessageType = MessageType.General,
                    ReceiverId = 0,
                    Route = new Channel[0],
                    SenderId = 1,
                    LastTransferNodeId = 2,
                    Data = null,
                    Size = 10,
                    ParentId = Guid.Empty,
                    SendAttempts = 2
                }
            };

            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(_messages);
        }

        public void ShowShouldAddAllMessagesToTree()
        {
            // Arrange
            // Act
            _messageViewUpdater.Show();

            // Assert
            Assert.That(_treeView.Items.Count, Is.EqualTo(_messages.Length));
        }
    }
}
