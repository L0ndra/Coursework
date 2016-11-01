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
    public class MessageGeneratorTests
    {
        private Mock<INetworkHandler> _networkMock;
        private Mock<IMessageCreator> _messageCreatorMock;
        private MessageGenerator _messageGenerator;
        private Channel[] _channels;
        private Node[] _nodes;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _messageCreatorMock = new Mock<IMessageCreator>();

            _messageGenerator = new MessageGenerator(_networkMock.Object, _messageCreatorMock.Object, 0.5);

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

            _messageCreatorMock.Setup(m => m.CreateMessages(It.IsAny<MessageInitializer>()))
                .Returns(new[] { new Message() });
        }

        [Test]
        [TestCase(1.1)]
        [TestCase(-1.1)]
        public void ConstrcutorShouldThrowExceptionIfErrorChanceIsIncorrect(double chance)
        {
            // Arrange
            // Act
            TestDelegate testDelegate =
                () => _messageGenerator = new MessageGenerator(_networkMock.Object, _messageCreatorMock.Object, 
                    chance);

            // Assert
            Assert.That(testDelegate, Throws.ArgumentException);
        }

        [Test]
        public void GenerateShouldNeverGenerateMessages()
        {
            // Arrange
            _messageGenerator = new MessageGenerator(_networkMock.Object, _messageCreatorMock.Object, 0.0);

            // Act
            for (var i = 0; i < 100; i++)
            {
                _messageGenerator.Generate();
            }

            // Assert
            Assert.IsNull(_messageGenerator.LastGeneratedMessage);
        }

        [Test]
        public void GenerateShouldGenerateMessage()
        {
            // Arrange
            _messageGenerator = new MessageGenerator(_networkMock.Object, _messageCreatorMock.Object, 1.0);

            // Act
            _messageGenerator.Generate();
            var message = _messageGenerator.LastGeneratedMessage;

            // Assert
            Assert.IsNotNull(message);
        }
    }
}
