using System;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.MessageServices;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageExchangerTests
    {
        private Mock<INetworkHandler> _networkMock;
        private IMessageExchanger _messageExchanger;
        private Node[] _nodes;
        private Channel[] _channels;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();

            _messageExchanger = new MessageExchanger(_networkMock.Object);

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
                    IsActive = true
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
        }

        [Test]
        public void InitializeShouldStartSendingInformationMessagesToMetropolitanNetworks()
        {
            // Arrange
            var centralMachine = _nodes.First(n => n.NodeType == NodeType.CentralMachine);

            // Act
            _messageExchanger.Initialize();

            // Assert
            Assert.IsTrue(centralMachine.MessageQueueHandlers
                .SelectMany(m => m.Messages)
                .All(m => m.MessageType == MessageType.InitializeMessage));

            foreach (var messageQueueHandler in centralMachine.MessageQueueHandlers)
            {
                Assert.That(messageQueueHandler.MessagesCount, Is.EqualTo(1));
            }
        }

        [Test]
        [Ignore("Method Not Implemented")]
        public void HandleMessagesOnceShouldReplaceMessageFromQueueToChannel()
        {
            Assert.Fail();
        }
    }
}
