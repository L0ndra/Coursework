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
    public class MessageRepositoryTests
    {
        private Mock<INetworkHandler> _networkMock;
        private IMessageRepository _messageRepository;
        private Node[] _nodes;
        private Channel[] _channels;
        private Node FirstNode => _nodes.First();

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();

            _messageRepository = new MessageRepository(_networkMock.Object);

            _nodes = new[]
            {
                new Node
                {
                    Id = 0,
                    MessageQueueHandlers = new List<MessageQueueHandler>
                    {
                        new MessageQueueHandler(Guid.Empty)
                    },
                    ReceivedMessages = new List<Message>
                    {
                        new Message()
                    }
                }
            };

            _channels = new[]
            {
                new Channel
                {
                    Id = Guid.Empty,
                    FirstNodeId = 0,
                    SecondNodeId = 1,
                    FirstMessage = new Message(),
                    SecondMessage = null,
                }
            };

            _networkMock.Setup(n => n.Nodes)
                .Returns(_nodes);

            _networkMock.Setup(n => n.Channels)
                .Returns(_channels);

            FirstNode.MessageQueueHandlers.First().AppendMessage(new Message());
        }

        [Test]
        public void GetAllMessagesShouldReturnAllMessagesInNetwork()
        {
            // Arrange
            var messageCountInNodes = _nodes.SelectMany(n => n.MessageQueueHandlers)
                .SelectMany(m => m.Messages)
                .Count();

            var messageCountInChannels = _channels.Select(c => c.FirstMessage).Count()
                + _channels.Select(c => c.SecondMessage).Count();

            var receivedMessages = _nodes.SelectMany(n => n.ReceivedMessages)
                .Count();

            // Act
            var result = _messageRepository.GetAllMessages();

            // Assert
            Assert.That(result.Length, Is.EqualTo(messageCountInChannels + messageCountInNodes
                + receivedMessages));
        }

        [Test]
        public void GetAllMessagesShouldReturnAllMessagesInSpecifiedNode()
        {
            // Arrange
            var messageCountInNode = FirstNode.MessageQueueHandlers
                .SelectMany(m => m.Messages)
                .Count();

            // Act
            var result = _messageRepository.GetAllMessages(FirstNode.Id);

            // Assert
            Assert.That(result.Length, Is.EqualTo(messageCountInNode));
        }

        [Test]
        public void GetAllMessagesShouldThrowExceptionIfNodeNotExists()
        {
            // Arrange
            // Act
            TestDelegate testDelegate = () => _messageRepository.GetAllMessages(uint.MaxValue);

            // Assert
            Assert.That(testDelegate, Throws.InvalidOperationException);
        }
    }
}
