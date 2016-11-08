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
        private Mock<IMessageCreator> _generalMessageCreatorMock;
        private Mock<IMessageCreator> _updateMatrixMessageCreatorMock;
        private Mock<IMessageCreator> _positiveResponseMessageCreatorMock;
        private MessageHandler _messageHandler;
        private Channel[] _channels;
        private Node[] _nodes;
        private Message _message;
        private Node Receiver => _nodes.First(n => n.Id == _message.ReceiverId);

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _generalMessageCreatorMock = new Mock<IMessageCreator>();
            _updateMatrixMessageCreatorMock = new Mock<IMessageCreator>();
            _positiveResponseMessageCreatorMock = new Mock<IMessageCreator>();

            _messageHandler = new MessageHandler(_networkMock.Object, _generalMessageCreatorMock.Object,
                _updateMatrixMessageCreatorMock.Object,
                _positiveResponseMessageCreatorMock.Object);


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
                    NodeType = NodeType.CentralMachine,
                    ReceivedMessages = new List<Message>(),
                    CanceledMessages = new List<Message>()
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
                    IsActive = false,
                    ReceivedMessages = new List<Message>(),
                    CanceledMessages = new List<Message>()
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
                    IsActive = false,
                    ReceivedMessages = new List<Message>(),
                    CanceledMessages = new List<Message>()
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
                    IsActive = false,
                    ReceivedMessages = new List<Message>(),
                    CanceledMessages = new List<Message>()
                },
                new Node
                {
                    Id = 4,
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueueHandlers = new List<MessageQueueHandler>(),
                    IsActive = false,
                    ReceivedMessages = new List<Message>(),
                    CanceledMessages = new List<Message>()
                }
            };

            _message = new Message
            {
                MessageType = MessageType.General,
                ReceiverId = 0,
                Route = new[]
                {
                    _channels[0]
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

            _positiveResponseMessageCreatorMock.Setup(m => m.CreateMessages(It.IsAny<MessageInitializer>()))
                .Returns(new[] 
                {
                    new Message
                    {
                        MessageType = MessageType.PositiveSendingResponse
                    }
                });
        }

        [Test]
        public void HandleMessageShouldCreateInitializeMessages()
        {
            // Arrange
            _message.MessageType = MessageType.MatrixUpdateMessage;
            _message.Data = new Dictionary<uint, NetworkMatrix>
            {
                [0] = NetworkMatrix.Initialize(_networkMock.Object),
                [1] = NetworkMatrix.Initialize(_networkMock.Object),
                [2] = NetworkMatrix.Initialize(_networkMock.Object),
                [3] = NetworkMatrix.Initialize(_networkMock.Object),
                [4] = NetworkMatrix.Initialize(_networkMock.Object),
            };

            var linkedNodeCount = Receiver.LinkedNodesId.Count;

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _updateMatrixMessageCreatorMock.Verify(c => c.CreateMessages(It.Is<MessageInitializer>
                (m => m.MessageType == MessageType.MatrixUpdateMessage)), Times.Exactly(linkedNodeCount));

            _updateMatrixMessageCreatorMock.Verify(c => c.AddInQueue(It.Is<Message[]>
                (m => m.All(m1 => m1.MessageType == MessageType.MatrixUpdateMessage)), Receiver.Id),
                Times.Exactly(linkedNodeCount));

            Assert.IsTrue(Receiver.IsActive);
            Assert.That(Receiver.NetworkMatrix,
                Is.EqualTo(((Dictionary<uint, NetworkMatrix>)_message.Data)[0]));
        }

        [Test]
        public void HandleMessagesShouldRemoveOldInitializeMessagesFromQueue()
        {
            // Arrange
            _message.MessageType = MessageType.MatrixUpdateMessage;
            _message.Data = new Dictionary<uint, NetworkMatrix>
            {
                [0] = NetworkMatrix.Initialize(_networkMock.Object),
                [1] = NetworkMatrix.Initialize(_networkMock.Object),
                [2] = NetworkMatrix.Initialize(_networkMock.Object),
                [3] = NetworkMatrix.Initialize(_networkMock.Object),
                [4] = NetworkMatrix.Initialize(_networkMock.Object),
            };

            var firstNode = _nodes.First();

            // Act
            _messageHandler.HandleMessage(_message);
            _messageHandler.HandleMessage(_message);

            var messageCount = firstNode.MessageQueueHandlers
                .SelectMany(m => m.Messages)
                .Count(m => m.MessageType == MessageType.MatrixUpdateMessage);

            // Assert
            Assert.That(messageCount, Is.Zero);
        }

        [Test]
        public void HandleMessageShouldRemoveItFromQueue()
        {
            // Arrange
            _message.MessageType = MessageType.General;

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _networkMock.Verify(n => n.RemoveFromQueue(_message, 0), Times.Once());
        }

        [Test]
        public void HandleMessageShouldReplaceReceivedMessage()
        {
            // Arrange
            var receiver = _nodes.First(n => n.Id == _message.ReceiverId);

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            Assert.IsTrue(receiver.ReceivedMessages.Contains(_message));
        }

        [Test]
        public void HandleMessageShouldHandleRequestMessage()
        {
            // Arrange
            _message.MessageType = MessageType.SendingRequest;
            _message.Data = new[] { _message };

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _positiveResponseMessageCreatorMock.Verify(c => c.CreateMessages(It.Is<MessageInitializer>
                (m => m.MessageType == MessageType.PositiveSendingResponse)), Times.Once);

            _positiveResponseMessageCreatorMock.Verify(c => c.AddInQueue(It.Is<Message[]>
                (m => m.All(m1 => m1.MessageType == MessageType.PositiveSendingResponse)), Receiver.Id),
                Times.Once);
        }

        [Test]
        public void HandleMessageShouldCreateResponsesWithParentIdEqualToRequestParentId()
        {
            // Arrange
            _message.MessageType = MessageType.SendingRequest;
            _message.Data = new[] { _message };

            // Act
            _messageHandler.HandleMessage(_message);

            var messagesInNetwork = _nodes.SelectMany(m => m.MessageQueueHandlers)
                .SelectMany(m => m.Messages);

            // Assert
            Assert.That(messagesInNetwork.All(m => m.ParentId == _message.ParentId));
        }

        [Test]
        public void HandleMessageShouldHandleResponseMessage()
        {
            // Arrange
            _message.MessageType = MessageType.PositiveSendingResponse;
            var innerMessage = new Message();
            _message.Data = new[] { innerMessage };

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _networkMock.Verify(n => n.AddInQueue(It.Is<Message>(m => m.MessageType == MessageType.General),
                _message.ReceiverId), Times.Once());

            _networkMock.Verify(n => n.RemoveFromQueue(_message, _message.ReceiverId), Times.Once);
        }

        [Test]
        public void HandleMessageShouldRepeatSendingMessagesInData()
        {
            // Arrange
            _message.MessageType = MessageType.NegativeSendingResponse;
            var innerMessage = new Message();
            _message.Data = new[] { innerMessage };

            // Act
            _messageHandler.HandleMessage(_message);

            // Assert
            _generalMessageCreatorMock.Verify(m => m.CreateMessages(It.IsAny<MessageInitializer>()), Times.Once);
            _generalMessageCreatorMock.Verify(m => m.AddInQueue(It.IsAny<Message[]>(), _message.ReceiverId), Times.Once);
        }
    }
}
