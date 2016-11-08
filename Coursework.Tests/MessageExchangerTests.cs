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
        private Mock<IMessageReceiver> _messageReceiverMock;
        private IMessageExchanger _messageExchanger;
        private Node[] _nodes;
        private Channel[] _channels;
        private Message _message;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();
            _messageReceiverMock = new Mock<IMessageReceiver>();

            _messageExchanger = new MessageExchanger(_networkMock.Object, _messageReceiverMock.Object);

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
                    Price = 10,
                    Capacity = 5
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
                    IsActive = true,
                    CanceledMessages = new List<Message>()
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
                Route = new[] { _channels.First() },
                DataSize = 20
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
        public void HandleMessagesOnceShouldReplaceMessageToChannelInFirstMessageSlot()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsNotNull(firstChannel.FirstMessage);
            Assert.IsNull(firstChannel.SecondMessage);
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageToChannelInSecondMessageSlot()
        {
            // Arrange
            var secondNode = _nodes.Skip(1).First();

            _message.LastTransferNodeId = 1;

            secondNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsNotNull(firstChannel.SecondMessage);
            Assert.IsNull(firstChannel.FirstMessage);
        }

        [Test]
        public void HandleMessagesOnceShouldDoNothingIfChannelIsBusy()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();

            firstChannel.IsBusy = true;
            firstChannel.MessageOwnerId = Guid.NewGuid();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsNull(firstChannel.FirstMessage);
            Assert.IsNull(firstChannel.SecondMessage);
            Assert.IsTrue(firstNode.MessageQueueHandlers
                .SelectMany(m => m.Messages)
                .Contains(_message));
        }

        [Test]
        public void HandleMessagesOnceShouldIncreaseFirstSlotReceivedDataSizeToCapacity()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();

            _messageExchanger.HandleMessagesOnce();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.That(firstChannel.FirstSlotReceivedData, Is.EqualTo(firstChannel.Capacity));
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageFromChannelToNextNode()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.FirstSlotReceivedData = _message.Size;
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
            Assert.That(firstChannel.FirstSlotReceivedData, Is.Zero);
            Assert.IsNull(firstChannel.FirstMessage);
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageBackToSenderIfChannelIsBusy()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.MessageOwnerId = Guid.NewGuid();
            firstChannel.FirstSlotReceivedData = _message.Size;
            firstChannel.ErrorChance = 0.0;

            _messageExchanger.HandleMessagesOnce();

            firstChannel.IsBusy = true;

            Func<Node, bool> checkPredicate = node => node.MessageQueueHandlers
                .First(m => m.ChannelId == firstChannel.Id)
                .Messages
                .Contains(_message);

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsTrue(checkPredicate(firstNode));
            Assert.That(firstChannel.FirstSlotReceivedData, Is.Zero);
            Assert.IsNull(firstChannel.FirstMessage);
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageToReceiverIfChannelIsBusyButMessageIsOwnerOfChannel()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.IsBusy = true;
            firstChannel.MessageOwnerId = _message.ParentId;
            firstChannel.FirstSlotReceivedData = _message.Size;
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
            Assert.That(firstChannel.FirstSlotReceivedData, Is.Zero);
            Assert.IsNull(firstChannel.FirstMessage);
        }

        [Test]
        public void HandleMessagesOnceShouldReplaceMessageFromChannelBackToSender()
        {
            // Arrange
            var firstNode = _nodes.First();

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.FirstSlotReceivedData = _message.Size;
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
            Assert.That(firstChannel.FirstSlotReceivedData, Is.Zero);
            Assert.IsNull(firstChannel.FirstMessage);
        }

        [Test]
        public void HandleMessagesShouldCallHandleMessageFromReceiverOnce()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            secondNode.IsActive = false;

            _message.MessageType = MessageType.MatrixUpdateMessage;

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.ErrorChance = 0.0;

            _messageExchanger.HandleMessagesOnce();
            firstChannel.FirstSlotReceivedData = _message.Size;
            _messageExchanger.HandleMessagesOnce();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            _messageReceiverMock.Verify(m => m.HandleReceivedMessage(secondNode, _message), Times.Once());
        }

        [Test]
        public void HandleMessagesInNodeQueuesShouldRemoveInitializeMessagesIfReceiverIsActive()
        {
            // Arrange
            var firstNode = _nodes.First();
            var secondNode = _nodes.Skip(1).First();

            secondNode.IsActive = true;

            _message.MessageType = MessageType.MatrixUpdateMessage;

            var mesageQueueHandler = firstNode.MessageQueueHandlers.First();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.That(mesageQueueHandler.Messages.Length, Is.Zero);
        }


        [Test]
        public void HandleMessagesShouldMakeChannelBusyIfMessageTypeIsSendingRequest()
        {
            // Arrange
            var firstNode = _nodes.First();

            _message.MessageType = MessageType.SendingRequest;
            _message.Data = new[] { _message };

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.ErrorChance = 0.0;

            _messageExchanger.HandleMessagesOnce();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsTrue(firstChannel.IsBusy);
            Assert.That(firstChannel.MessageOwnerId, Is.EqualTo(_message.ParentId));
        }

        [Test]
        public void HandleMessagesShouldFreeChannelIfMessageTypeIsGeneral()
        {
            // Arrange
            var firstNode = _nodes.First();

            _message.MessageType = MessageType.General;

            firstNode.MessageQueueHandlers
                .First().AppendMessage(_message);

            var firstChannel = _channels.First();
            firstChannel.IsBusy = true;
            firstChannel.FirstSlotReceivedData = _message.Size;
            firstChannel.MessageOwnerId = _message.ParentId;
            firstChannel.ErrorChance = 0.0;

            _messageExchanger.HandleMessagesOnce();

            // Act
            _messageExchanger.HandleMessagesOnce();

            // Assert
            Assert.IsFalse(firstChannel.IsBusy);
        }
    }
}
