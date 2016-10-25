using System;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageQueueHandlerTests
    {
        private IMessageQueueHandler _messageQueueHandler;
        private Message _message;

        [SetUp]
        public void Setup()
        {
            _messageQueueHandler = new MessageQueueHandler(Guid.Empty);

            _message = new Message
            {
                ReceiverId = 5,
                SenderId = 4,
                Size = 20
            };
        }

        [Test]
        public void AddMessageShouldAddMessageInQueue()
        {
            // Arrange
            var initialSize = _messageQueueHandler.MessagesCount;

            // Act
            _messageQueueHandler.AddMessage(_message);
            var resultSize = _messageQueueHandler.MessagesCount;

            // Assert
            Assert.That(resultSize - initialSize, Is.EqualTo(1));
        }
    }
}
