using System;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
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
                DataSize = 1,
                ServiceSize = 1,
                Route = new Channel[0]
            };
        }

        [Test]
        public void AppendMessageShouldAddMessageInQueue()
        {
            // Arrange
            var initialSize = _messageQueueHandler.MessagesCount;

            // Act
            _messageQueueHandler.AppendMessage(_message);
            var resultSize = _messageQueueHandler.MessagesCount;

            // Assert
            Assert.That(resultSize - initialSize, Is.EqualTo(1));
        }

        [Test]
        public void AppendMessageShouldThrowExceptionIfDataSizeIsLessThanZero()
        {
            // Arrange
            _message.DataSize = -1;

            // Act
            TestDelegate testDelegate = () => _messageQueueHandler.AppendMessage(_message);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(MessageException)));
        }

        [Test]
        public void AppendMessageShouldThrowExceptionIfServiceSizeIsLessThanZero()
        {
            // Arrange
            _message.ServiceSize = -2;

            // Act
            TestDelegate testDelegate = () => _messageQueueHandler.AppendMessage(_message);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(MessageException)));
        }

        [Test]
        public void RemoveMessageShouldDoIt()
        {
            // Arrange
            _messageQueueHandler.AppendMessage(_message);
            var initialSize = _messageQueueHandler.MessagesCount;
            var messageToRemove = _message;
            
            // Act
            _messageQueueHandler.RemoveMessage(messageToRemove);
            var currentSize = _messageQueueHandler.MessagesCount;
            
            // Assert
            Assert.That(currentSize, Is.EqualTo(initialSize - 1));
        }

        [Test]
        public void RemoveMessageShouldDoNothingIfMessageNotExists()
        {
            // Arrange
            var initialSize = _messageQueueHandler.MessagesCount;
            var messageToRemove = _message;

            // Act
            _messageQueueHandler.RemoveMessage(messageToRemove);
            var currentSize = _messageQueueHandler.MessagesCount;

            // Assert
            Assert.That(currentSize, Is.EqualTo(initialSize));
        }

        [Test]
        public void AddMessageInStartShouldAddMessageOnZeroPosition()
        {
            // Arrange
            var newMessage = new Message
            {
                DataSize = 1,
                ServiceSize = 1,
                Route = new Channel[0]
            };

            _messageQueueHandler.AppendMessage(newMessage);

            var initialSize = _messageQueueHandler.MessagesCount;

            // Act
            _messageQueueHandler.AddMessageInStart(_message);

            var resultSize = _messageQueueHandler.MessagesCount;

            // Assert
            Assert.That(resultSize - initialSize, Is.EqualTo(1));
            Assert.That(_messageQueueHandler.Messages.First(), Is.EqualTo(_message));
        }

        [Test]
        public void AddMessageShouldThrowExceptionIfMessageRouteIsNull()
        {
            // Arrange
            _message.Route = null;

            // Act
            TestDelegate testDelegate = () => _messageQueueHandler.AppendMessage(_message);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(MessageException)));
        }
    }
}
