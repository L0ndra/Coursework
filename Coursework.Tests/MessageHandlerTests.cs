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
    public class MessageHandlerTests
    {
        private Mock<INetworkHandler> _networkMock;
        private Mock<IMessageCreator> _messageCreatorMock;
        private Mock<IMessageRouter> _messageRouter;
        private MessageHandler _messageHandler;
        private Channel[] _channels;
        private Node[] _nodes;
        private Message _message;
        private Node Receiver => _nodes.First(n => n.Id == _message.ReceiverId);

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _messageCreatorMock = new Mock<IMessageCreator>();
            _messageRouter = new Mock<IMessageRouter>();

            _messageHandler = new MessageHandler(_networkMock.Object, _messageCreatorMock.Object,
                _messageRouter.Object);

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
                    IsActive = false,
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
                    IsActive = false
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
                    IsActive = false
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
                    IsActive = false
                },
                new Node
                {
                    Id = 4,
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueueHandlers = new List<MessageQueueHandler>(),
                    IsActive = false
                }
            };

            _message = new Message
            {
                MessageType = MessageType.General,
                ReceiverId = 0,
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
        public void HandleMessageShouldCreateInitializeMessages()
        {
            // Arrange
            _message.MessageType = MessageType.InitializeMessage;
            var linkedNodeCount = Receiver.LinkedNodesId.Count;

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _messageCreatorMock.Verify(m => m.AddInQueue(It.Is<Message[]>
                (m1 => m1.All(message => message.MessageType == MessageType.InitializeMessage))),
                Times.Exactly(linkedNodeCount));

            Assert.IsTrue(Receiver.IsActive);
        }

        [Test]
        public void HandleMessageShouldCorrectHandleSendingRequest()
        {
            // Arrange
            _message.MessageType = MessageType.SendingRequest;
            _message.Data = new[]
            {
                new Message()
            };

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _messageCreatorMock.Verify(m => m.CreateMessages(It.Is<MessageInitializer>
                (m1 => m1.MessageType == MessageType.SendingResponse)),
                Times.Exactly(1));

            _messageCreatorMock.Verify(m => m.AddInQueue(It.Is<Message[]>
                (m1 => m1.All(message => message.MessageType == MessageType.SendingResponse))),
                Times.Exactly(1));
        }

        [Test]
        public void HandleMessageShouldDoNothingIfReceiverIsNotCentralMachine()
        {
            // Arrange
            _message.MessageType = MessageType.SendingRequest;
            Receiver.NodeType = NodeType.SimpleNode;

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _messageCreatorMock.Verify(m => m.CreateMessages(It.Is<MessageInitializer>
                (m1 => m1.MessageType == MessageType.SendingResponse)),
                Times.Never());

            _messageCreatorMock.Verify(m => m.AddInQueue(It.Is<Message[]>
                (m1 => m1.All(message => message.MessageType == MessageType.SendingResponse))),
                Times.Never());
        }

        [Test]
        public void HandleMessageShouldAddInQueueAllMessagesFromData()
        {
            // Arrange
            _message.MessageType = MessageType.SendingResponse;
            _message.Data = new[]
            {
                new Message()
            };

            var messagesCount = ((Message[])_message.Data).Length;

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _messageCreatorMock.Verify(m => m.AddInQueue(It.IsAny<Message[]>()),
                Times.Exactly(messagesCount));
        }
    }
}
