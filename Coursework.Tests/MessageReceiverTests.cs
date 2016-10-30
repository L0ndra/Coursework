using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageReceiverTests
    {
        private Mock<INetworkHandler> _networkMock;
        private Mock<IMessageSender> _messageSenderMock;
        private IMessageReceiver _messageReceiver;
        private Node _node;
        private Message _message;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _messageSenderMock = new Mock<IMessageSender>();

            _messageReceiver = new MessageReceiver(_networkMock.Object, _messageSenderMock.Object);

            _message = new Message
            {
                SenderId = 1,
                LastTransferNodeId = 0,
                ReceiverId = 0,
                MessageType = MessageType.InitializeMessage,
                Route = new[]
                {
                    new Channel()
                }
            };

            _node = new Node
            {
                Id = 0,
                MessageQueueHandlers = new List<MessageQueueHandler>
                {
                    new MessageQueueHandler(Guid.Empty)
                },
                LinkedNodesId = new SortedSet<uint>(new uint[] { 1 }),
                IsActive = false,
                NodeType = NodeType.SimpleNode
            };

            _node.MessageQueueHandlers
                .First().AddMessage(_message);

            _networkMock.Setup(n => n.GetNodeById(It.IsAny<uint>()))
                .Returns((uint id) => id == 0 ? _node : new Node { Id = 1, IsActive = false });
        }

        [Test]
        public void HandleReceivedMessageShouldActivateNodeIfReceiverMessageIsInitialized()
        {
            // Arrange
            // Act
            _messageReceiver.HandleReceivedMessage(_node, _message);

            // Assert
            Assert.IsTrue(_node.IsActive);
        }

        [Test]
        public void HandleReceivedMessageShouldCreateNewInitializeMessagesToUnactiveLinkedNodes()
        {
            // Arrange
            var linkedNodesCount = _node.LinkedNodesId.Count;

            // Act
            _messageReceiver.HandleReceivedMessage(_node, _message);

            var messagesCount = _node.MessageQueueHandlers.First().Messages.Length;

            // Assert
            Assert.That(messagesCount, Is.EqualTo(1));
            _messageSenderMock.Verify(m => m.StartSendProcess(It.Is<MessageInitializer>
                (m1 => m1.MessageType == MessageType.InitializeMessage)), Times.Exactly(linkedNodesCount));
        }
    }
}
