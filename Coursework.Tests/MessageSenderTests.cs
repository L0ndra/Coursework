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
    public class MessageSenderTests
    {
        private Mock<INetworkHandler> _networkMock;
        private IMessageSender _messageSender;
        private MessageInitializer _messageInitializer;
        private Node[] _nodes;
        private Channel[] _channels;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();

            _messageSender = new MessageSender(_networkMock.Object);

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

            _networkMock.Setup(n => n.Nodes)
                .Returns(_nodes);

            _networkMock.Setup(n => n.GetNodeById(It.IsAny<uint>()))
                .Returns((uint nodeId) => _nodes.FirstOrDefault(n => n.Id == nodeId));

            _networkMock.Setup(n => n.Channels)
                .Returns(_channels);

            _networkMock.Setup(n => n.GetChannel(It.IsAny<uint>(), It.IsAny<uint>()))
                .Returns(_channels.First());

            _messageInitializer = new MessageInitializer
            {
                MessageType = MessageType.InitializeMessage,
                ReceiverId = 1,
                SenderId = 0,
                Data = null,
                Size = 5
            };
        }

        [Test]
        public void StartSendProcessShouldCreateInitializeMessage()
        {
            // Arrange
            var firstNode = _nodes.First();
            var messageQueue = firstNode.MessageQueueHandlers.First();

            // Act
            _messageSender.StartSendProcess(_messageInitializer);

            // Assert
            Assert.That(messageQueue.MessagesCount, Is.EqualTo(1));
        }
    }
}
