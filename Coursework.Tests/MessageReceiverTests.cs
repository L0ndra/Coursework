using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageReceiverTests
    {
        private Mock<IMessageHandler> _messageHandlerMock;
        private IMessageReceiver _messageReceiver;
        private Message _message;
        private Node _node;

        [SetUp]
        public void Setup()
        {
            _messageHandlerMock = new Mock<IMessageHandler>();
            _messageReceiver = new MessageReceiver(_messageHandlerMock.Object);

            _message = new Message
            {
                SenderId = 1,
                LastTransferNodeId = 0,
                ReceiverId = 0,
                MessageType = MessageType.InitializeMessage,
                Route = new[]
                {
                    new Channel(),
                    new Channel()
                }
            };

            _node = new Node
            {
                Id = 2,
                MessageQueueHandlers = new List<MessageQueueHandler>
                {
                    new MessageQueueHandler(Guid.Empty)
                },
                IsActive = false,
                NodeType = NodeType.SimpleNode
            };
        }

        [Test]
        public void HandleReceivedMessageShouldCallHandleMessageFromHandlerOnceIfNodeIsReceiver()
        {
            // Arrange
            _message.Route = _message.Route
                .Skip(1)
                .ToArray();

            // Act
            _messageReceiver.HandleReceivedMessage(_node, _message);

            // Assert
            _messageHandlerMock.Verify(m => m.HandleMessage(_message), Times.Once());
        }

        [Test]
        public void HandleReceivedMessageShouldReplaceMessageToAnotherQueue()
        {
            // Arrange
            // Act
            _messageReceiver.HandleReceivedMessage(_node, _message);

            var messagesCount = _node.MessageQueueHandlers.First().Messages.Length;

            // Assert
            Assert.That(messagesCount, Is.EqualTo(1));
        }
    }
}
