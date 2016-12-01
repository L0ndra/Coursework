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
        private Mock<IMessageCreator> _negativeResponseMessageCreatorMock;
        private IMessageReceiver _messageReceiver;
        private Message _message;
        private Node _node;

        [SetUp]
        public void Setup()
        {
            _messageHandlerMock = new Mock<IMessageHandler>();
            _negativeResponseMessageCreatorMock = new Mock<IMessageCreator>();

            _messageReceiver = new MessageReceiver(_messageHandlerMock.Object,
                _negativeResponseMessageCreatorMock.Object);

            _message = new Message
            {
                SenderId = 1,
                LastTransferNodeId = 0,
                ReceiverId = 0,
                MessageType = MessageType.MatrixUpdateMessage,
                Route = new[]
                {
                    new Channel(),
                    new Channel()
                },
                Data = null,
            };

            _node = new Node
            {
                Id = 2,
                MessageQueueHandlers = new List<MessageQueueHandler>
                {
                    new MessageQueueHandler(Guid.Empty)
                },
                IsActive = false,
                NodeType = NodeType.SimpleNode,
                CanceledMessages = new List<Message>()
            };

            _negativeResponseMessageCreatorMock.Setup(n => n.CreateMessages(It.IsAny<MessageInitializer>()))
                .Returns(new[]
                {
                    new Message
                    {
                        MessageType = MessageType.NegativeSendingResponse,
                        Route = new [] {new Channel()}
                    }
                });
        }

        [Test]
        public void HandleReceivedMessageShouldCallHandleMessageFromHandlerOnceIfNodeIsReceiver()
        {
            // Arrange
            _message.ReceiverId = _node.Id;

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

        [Test]
        public void HandleReceivedMessageShouldCreateNegativeResponseIfMessageIsRequest()
        {
            // Arrange
            _message.MessageType = MessageType.SendingRequest;
            _message.Data = new Message
            {
                Route = new[]
                {
                    new Channel()
                }
            };

            var passedChannel = _message.Route.First();
            passedChannel.IsBusy = true;
            passedChannel.MessageOwnerId = Guid.NewGuid();

            var transferId = _message.LastTransferNodeId;

            // Act
            _messageReceiver.HandleReceivedMessage(_node, _message);

            var nodeMessages = _node.MessageQueueHandlers.SelectMany(m => m.Messages);

            // Assert
            _negativeResponseMessageCreatorMock.Verify(c => c.CreateMessages(It.Is<MessageInitializer>
                (m => m.MessageType == MessageType.NegativeSendingResponse)), Times.Once);

            _negativeResponseMessageCreatorMock.Verify(c => c.AddInQueue(It.Is<Message[]>
                (m => m.All(m1 => m1.MessageType == MessageType.NegativeSendingResponse)), transferId),
                Times.Once);

            Assert.That(nodeMessages.All(m => m.MessageType == MessageType.NegativeSendingResponse));
        }

        [Test]
        public void HandleReceivedMessageShouldHandleNegativeResponseAtTheSameTimeItCreatesIt()
        {
            // Arrange
            _message.MessageType = MessageType.SendingRequest;
            _negativeResponseMessageCreatorMock.Setup(n => n.CreateMessages(It.IsAny<MessageInitializer>()))
                .Returns(new[]
                {
                    new Message
                    {
                        MessageType = MessageType.NegativeSendingResponse,
                        Route = new Channel[0]
                    }
                });

            var passedChannel = _message.Route.First();
            passedChannel.IsBusy = true;
            passedChannel.MessageOwnerId = Guid.NewGuid();

            // Act
            _messageReceiver.HandleReceivedMessage(_node, _message);

            // Assert
            _negativeResponseMessageCreatorMock.Verify(c => c.CreateMessages(It.Is<MessageInitializer>
                (m => m.MessageType == MessageType.NegativeSendingResponse)), Times.Once);

            _messageHandlerMock.Verify(m => m.HandleMessage(It.Is<Message>
                (m1 => m1.MessageType == MessageType.NegativeSendingResponse)), Times.Once());
        }
    }
}
