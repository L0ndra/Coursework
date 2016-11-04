using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageCreatorTests
    {
        private Mock<INetworkHandler> _networkMock;
        private Mock<IMessageRouter> _messageRouterMock;
        private IMessageCreator _messageCreator;
        private MessageInitializer _messageInitializer;
        private Node[] _nodes;
        private Channel[] _channels;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _messageRouterMock = new Mock<IMessageRouter>();

            _messageCreator = new MessageCreator(_networkMock.Object, _messageRouterMock.Object);

            _channels = new[]
            {
                new Channel
                {
                    Id = Guid.NewGuid(),
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Ground,
                    FirstNodeId = 0,
                    ErrorChance = 0.5,
                    SecondNodeId = 1,
                    Price = 10
                },
                new Channel
                {
                    Id = Guid.NewGuid(),
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Ground,
                    FirstNodeId = 0,
                    ErrorChance = 0.5,
                    SecondNodeId = 2,
                    Price = 20
                },
                new Channel
                {
                    Id = Guid.NewGuid(),
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Ground,
                    FirstNodeId = 1,
                    ErrorChance = 0.5,
                    SecondNodeId = 3,
                    Price = 100
                },
                new Channel
                {
                    Id = Guid.NewGuid(),
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Ground,
                    FirstNodeId = 2,
                    ErrorChance = 0.5,
                    SecondNodeId = 3,
                    Price = 1
                },
            };

            _nodes = new[]
            {
                new Node
                {
                    Id = 0,
                    LinkedNodesId = new SortedSet<uint>(new uint[] {1, 2}),
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(_channels[0].Id),
                        new MessageQueueHandler(_channels[1].Id)
                    },
                    IsActive = true,
                    NodeType = NodeType.CentralMachine
                },
                new Node
                {
                    Id = 1,
                    LinkedNodesId = new SortedSet<uint>(new uint[] {0, 3}),
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(_channels[0].Id),
                        new MessageQueueHandler(_channels[2].Id)
                    },
                    IsActive = true
                },
                new Node
                {
                    Id = 2,
                    LinkedNodesId = new SortedSet<uint>(new uint[] {0, 3}),
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(_channels[1].Id),
                        new MessageQueueHandler(_channels[3].Id)
                    },
                    IsActive = true
                },
                new Node
                {
                    Id = 3,
                    LinkedNodesId = new SortedSet<uint>(new uint[] {1, 2}),
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(_channels[2].Id),
                        new MessageQueueHandler(_channels[3].Id)
                    },
                    IsActive = true
                }
            };

            _networkMock.Setup(n => n.Nodes)
                .Returns(_nodes);

            _networkMock.Setup(n => n.Channels)
                .Returns(_channels);

            _networkMock.Setup(n => n.GetNodeById(It.IsAny<uint>()))
                .Returns((uint nodeId) => _nodes.FirstOrDefault(n => n.Id == nodeId));

            _networkMock.Setup(n => n.GetChannel(It.IsAny<uint>(), It.IsAny<uint>()))
                .Returns((uint firstNodeId, uint secondNodeId) =>
                {
                    return _channels.FirstOrDefault(c => c.FirstNodeId == firstNodeId && c.SecondNodeId == secondNodeId
                                                         ||
                                                         c.FirstNodeId == secondNodeId && c.SecondNodeId == firstNodeId);
                }
            );

            _messageRouterMock.Setup(m => m.GetRoute(It.IsAny<uint>(), It.IsAny<uint>()))
                .Returns((uint senderId, uint receiverId) =>
                {
                    var sender = _nodes.First(n => n.Id == senderId);

                    return new[]
                    {
                        _channels.First(c => c.Id == sender.MessageQueueHandlers.First().ChannelId)
                    };
                });

            _messageInitializer = new MessageInitializer
            {
                MessageType = MessageType.General,
                ReceiverId = 2,
                SenderId = 0,
                Data = null,
                Size = AllConstants.MaxMessageSize
            };
        }

        [Test]
        public void UpdateTablesShouldStartSendingInformationMessagesToMetropolitanNetworks()
        {
            // Arrange
            var centralMachine = _nodes.First(n => n.NodeType == NodeType.CentralMachine);

            // Act
            _messageCreator.UpdateTables();

            var messageCount = centralMachine.MessageQueueHandlers
                .SelectMany(m => m.Messages)
                .Count(m => m.MessageType == MessageType.MatrixUpdateMessage);

            // Assert
            Assert.That(messageCount, Is.EqualTo(messageCount));

            Assert.IsTrue(centralMachine.IsActive);
        }

        [Test]
        public void UpdateTablesShouldDoAllNodesOutdated()
        {
            // Arrange
            // Act
            _messageCreator.UpdateTables();

            // Assert
            Assert.IsTrue(_nodes
                .Where(n => n.NodeType != NodeType.CentralMachine)
                .All(n => !n.IsTableUpdated));
        }

        [Test]
        public void CreateMessagesShouldDoIt()
        {
            // Arrange
            // Act
            var messages = _messageCreator.CreateMessages(_messageInitializer);
            var firstMessage = messages.First();

            // Assert
            Assert.That(messages.Length, Is.EqualTo(1));
            Assert.That(firstMessage.ReceiverId, Is.EqualTo(_messageInitializer.ReceiverId));
            Assert.That(firstMessage.SenderId, Is.EqualTo(_messageInitializer.SenderId));
            Assert.That(firstMessage.Data, Is.EqualTo(_messageInitializer.Data));
            Assert.That(firstMessage.MessageType, Is.EqualTo(_messageInitializer.MessageType));
            Assert.That(firstMessage.DataSize, Is.EqualTo(_messageInitializer.Size));
            Assert.That(firstMessage.Size, Is.EqualTo(_messageInitializer.Size));
            Assert.That(firstMessage.ServiceSize, Is.Zero);
        }

        [Test]
        public void CreateMessageShouldDoNothingIfRouteIsNull()
        {
            // Arrange
            _messageRouterMock.Setup(m => m.GetRoute(It.IsAny<uint>(), It.IsAny<uint>()))
                .Returns((Channel[])null);

            // Act
            var messages = _messageCreator.CreateMessages(_messageInitializer);

            // Assert
            Assert.IsNull(messages);
        }

        [Test]
        public void AddInQueueShouldDoIt()
        {
            // Arrange
            var messages = _messageCreator.CreateMessages(_messageInitializer);

            // Act
            _messageCreator.AddInQueue(messages, 0);

            // Assert
            _networkMock.Verify(n => n.AddInQueue(It.IsAny<Message>(), It.IsAny<uint>()), 
                Times.Exactly(messages.Length));
        }

        [Test]
        public void RemoveFromQueueShouldDoIt()
        {
            // Arrange
            var messages = _messageCreator.CreateMessages(_messageInitializer);
            _messageCreator.AddInQueue(messages, 0);

            // Act
            _messageCreator.RemoveFromQueue(messages, 0);

            // Assert
            _networkMock.Verify(n => n.RemoveFromQueue(It.IsAny<Message>(), It.IsAny<uint>()), 
                Times.Exactly(messages.Length));
        }
    }
}
