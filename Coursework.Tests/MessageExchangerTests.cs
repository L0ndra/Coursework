using System;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.MessageServices;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageExchangerTests
    {
        private Mock<INetworkHandler> _networkMock;
        private Mock<IMessageSender> _messageSenderMock;
        private Mock<IMessageReceiver> _messageReceiverMock;
        private IMessageExchanger _messageExchanger;
        private Node[] _nodes;
        private Channel[] _channels;
        private Message _message;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _messageSenderMock = new Mock<IMessageSender>();
            _messageReceiverMock = new Mock<IMessageReceiver>();

            _messageExchanger = new MessageExchanger(_networkMock.Object, _messageSenderMock.Object,
                _messageReceiverMock.Object);

            _channels = new[]
            {
                new Channel
                {
                    Id = Guid.Empty,
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Ground,
                    FirstNodeId = 0,
                    SecondNodeId = 1,
                    ErrorChance = 0.5,
                    Price = 10
                }
            };

            _nodes = new[]
            {
                new Node
                {
                    Id = 0,
                    LinkedNodesId = new SortedSet<uint>(new uint[]{1}),
                    NodeType = NodeType.MainMetropolitanMachine,
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(Guid.Empty)
                    },
                    IsActive = true
                },
                new Node
                {
                    Id = 1,
                    LinkedNodesId = new SortedSet<uint>(new uint[]{0}),
                    NodeType = NodeType.CentralMachine,
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(Guid.Empty)
                    },
                    IsActive = false
                }
            };

            _message = new Message
            {
                SenderId = 0,
                LastTransferNodeId = 0,
                ReceiverId = 1,
            };

            _networkMock.Setup(n => n.Nodes)
                .Returns(_nodes);

            _networkMock.Setup(n => n.GetNodeById(It.IsAny<uint>()))
                .Returns((uint nodeId) => _nodes.FirstOrDefault(n => n.Id == nodeId));

            _networkMock.Setup(n => n.Channels)
                .Returns(_channels);

            _networkMock.Setup(n => n.GetChannel(It.IsAny<uint>(), It.IsAny<uint>()))
                .Returns(_channels.First());
        }

        [Test]
        public void InitializeShouldStartSendingInformationMessagesToMetropolitanNetworks()
        {
            // Arrange
            var centralMachine = _nodes.First(n => n.NodeType == NodeType.CentralMachine);
            var linkedNodesCount = centralMachine.LinkedNodesId.Count;

            // Act
            _messageExchanger.Initialize();

            // Assert
            _messageSenderMock.Verify(m => m.StartSendProcess(It.IsAny<MessageInitializer>()), 
                Times.Exactly(linkedNodesCount));
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageToChannel()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AddMessage(_message);

            var firstChannel = _channels.First();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsNotNull(firstChannel.FirstMessage);
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageFromChannelToNextNode()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            firstNode.MessageQueueHandlers
                .First().AddMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.ErrorChance = 0.0;

            _messageExchanger.HandleMessagesOnce();

            Func<Node, bool> checkPredicate = node => node.MessageQueueHandlers
                .First(m => m.ChannelId == firstChannel.Id)
                .Messages
                .Contains(_message);

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsTrue(checkPredicate(secondNode));
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageFromChannelBackToSender()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AddMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.ErrorChance = 1.0;

            _messageExchanger.HandleMessagesOnce();

            Func<Node, bool> checkPredicate = node => node.MessageQueueHandlers
                .First(m => m.ChannelId == firstChannel.Id)
                .Messages
                .Contains(_message);

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsTrue(checkPredicate(firstNode));
        }

        [Test]
        public void HandleMessagesShouldCallHandleMessageFromReceiverOnce()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            secondNode.IsActive = false;

            _message.MessageType = MessageType.InitializeMessage;

            firstNode.MessageQueueHandlers
                .First().AddMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.ErrorChance = 0.0;

            _messageExchanger.HandleMessagesOnce();
            _messageExchanger.HandleMessagesOnce();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            _messageReceiverMock.Verify(m => m.HandleReceivedMessage(secondNode, _message), Times.Once());
        }

        [Test]
        public void HandleMessagesInNodeQueuesShouldRemoveMessagesThatWasFailed()
        {
            // Arrange
            _message.SendAttempts = AllConstants.MaxAttempts + 1;
            var firstNode = _nodes.First();

            var mesageQueueHandler = firstNode.MessageQueueHandlers.First();

            mesageQueueHandler.AddMessage(_message);
            
            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.That(mesageQueueHandler.Messages.Length, Is.Zero);
        }

        [Test]
        public void HandleMessagesInNodeQueuesShouldRemoveInitializeMessagesIfReceiverIsActive()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            secondNode.IsActive = true;

            _message.MessageType = MessageType.InitializeMessage;

            var mesageQueueHandler = firstNode.MessageQueueHandlers.First();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.That(mesageQueueHandler.Messages.Length, Is.Zero);
        }
    }
}
