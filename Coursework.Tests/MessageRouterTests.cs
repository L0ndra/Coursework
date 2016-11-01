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
    public class MessageRouterTests
    {
        private Mock<INetworkHandler> _networkMock;
        private IMessageRouter _messageRouter;
        private Channel[] _channels;
        private Node[] _nodes;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();

            _messageRouter = new MessageRouter(_networkMock.Object);

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
                    IsActive = true
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
                },
                new Node
                {
                    Id = 4,
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueueHandlers = new List<MessageQueueHandler>(),
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
        }

        [Test]
        public void GetRouteShouldReturnOptimalRouteToNode()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.NetworkMatrix = _messageRouter.CountPriceMatrix(firstNode.Id);

            // Act
            var result = _messageRouter.GetRoute(0, 3);

            // Assert
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(_channels[1]));
            Assert.That(result[1], Is.EqualTo(_channels[3]));
        }

        [Test]
        public void GetRouteShouldReturnNullIfRouteNotExists()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.NetworkMatrix = _messageRouter.CountPriceMatrix(firstNode.Id);

            // Act
            var result = _messageRouter.GetRoute(0, 4);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetRouteShouldReturnEmptyArrayIfStartNodeAndDestinationAreEqual()
        {
            // Arrange
            // Act
            var result = _messageRouter.GetRoute(0, 0);

            // Assert
            Assert.That(result.Length, Is.Zero);
        }

        [Test]
        public void CountPriceShouldReturnCorrectPrice()
        {
            // Arrange
            var channel = _channels.First();

            var firstNode = _nodes.First(n => n.Id == channel.FirstNodeId);

            var messageQueue = firstNode.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            var realPrice = channel.Price
                            * (channel.ErrorChance + 1)
                            * (channel.ErrorChance + 1)
                            * (messageQueue.Messages.Length + 1)
                            * (messageQueue.Messages.Length + 1);

            // Act
            var result = _messageRouter.CountPrice(channel.FirstNodeId, channel.SecondNodeId);

            // Assert
            Assert.That(Math.Abs(result - realPrice), Is.LessThanOrEqualTo(AllConstants.Eps));
        }

        [Test]
        public void CountPriceShouldReturnPositiveInfiniteIfChannelNotExists()
        {
            // Arrange
            var channel = _channels.First();

            // Act
            var result = _messageRouter.CountPrice(channel.FirstNodeId, 3);

            // Assert
            Assert.IsTrue(double.IsInfinity(result));
        }

        [Test]
        public void CountPriceShouldReturnZeroIfSenderAndReceiverIsTheSameNode()
        {
            // Arrange
            // Act
            var result = _messageRouter.CountPrice(0, 0);

            // Assert
            Assert.That(result, Is.Zero);
        }
    }
}
