using System;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageRegistratorTests
    {
        private Mock<IMessageRepository> _messageRepoMock;
        private MessageRegistrator _messageRegistrator;
        private Message _unfinishedMessage;
        private Message _finishedMessage;

        [SetUp]
        public void Setup()
        {
            _messageRepoMock = new Mock<IMessageRepository>();

            _messageRegistrator = new MessageRegistrator(_messageRepoMock.Object);

            _unfinishedMessage = new Message
            {
                ParentId = Guid.NewGuid(),
                Route = new Channel[1],
                LastTransferNodeId = 0,
                ReceiverId = 1
            };

            _finishedMessage = new Message
            {
                ParentId = _unfinishedMessage.ParentId,
                IsReceived = true
            };

            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(new[] { _unfinishedMessage });
        }

        [Test]
        public void RegisterMessagesShouldCreateNewRecordForNewMessagesInRepo()
        {
            // Arrange
            // Act
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesStartTimes;

            // Assert
            Assert.IsTrue(registeredMessages.Keys.Contains(_unfinishedMessage.ParentId));
        }

        [Test]
        public void RegisterMessagesShouldCreateNewRecordWithCorrectRagistratorTactTime()
        {
            // Arrange
            var currentTactTime = _messageRegistrator.RegisterTact;

            // Act
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesStartTimes;

            // Assert
            Assert.That(registeredMessages[_unfinishedMessage.ParentId], Is.EqualTo(currentTactTime));
        }

        [Test]
        public void RegisterMessagesShouldIncrementRegistratorTactTime()
        {
            // Arrange
            var startTactTime = _messageRegistrator.RegisterTact;

            // Act
            _messageRegistrator.RegisterMessages();

            var endTactTime = _messageRegistrator.RegisterTact;

            // Assert
            Assert.That(endTactTime, Is.EqualTo(startTactTime + 1));
        }

        [Test]
        public void RegisterMessagesShouldNotRegisterUnfinishedMessageAsFinished()
        {
            // Arrange
            // Act
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesEndTimes;

            // Assert
            Assert.IsFalse(registeredMessages.Keys.Contains(_unfinishedMessage.ParentId));
        }

        [Test]
        public void RegisterMessagesShouldNotRegisterFinishedMessageIfAnotherMessageFromGroupIsUnfinished()
        {
            // Arrange
            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(new[] { _unfinishedMessage, _finishedMessage });

            // Act
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesEndTimes;

            // Assert
            Assert.IsFalse(registeredMessages.Keys.Contains(_unfinishedMessage.ParentId));
        }

        [Test]
        public void RegisterMessagesShouldRegisterFinishedMessageIfAllGroupIsFinished()
        {
            // Arrange
            _unfinishedMessage.IsReceived = true;

            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(new[] { _unfinishedMessage, _finishedMessage });

            // Act
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesEndTimes;

            // Assert
            Assert.IsTrue(registeredMessages.Keys.Contains(_unfinishedMessage.ParentId));
        }

        [Test]
        public void RegisterMessagesShouldRegisterFinishedMessageIfMessagesIsCanceledOrReceived()
        {
            // Arrange
            _unfinishedMessage.IsCanceled = true;

            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(new[] { _unfinishedMessage, _finishedMessage });

            // Act
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesEndTimes;

            // Assert
            Assert.IsTrue(registeredMessages.Keys.Contains(_unfinishedMessage.ParentId));
        }

        [Test]
        public void RegisterMessagesShouldRegisterFinishedMessageOnlyOnce()
        {
            // Arrange
            _unfinishedMessage.IsCanceled = true;

            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(new[] { _unfinishedMessage, _finishedMessage });

            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesEndTimes;

            var initialFinishTime = registeredMessages[_unfinishedMessage.ParentId];

            // Act
            _messageRegistrator.RegisterMessages();

            var resultFinishTime = registeredMessages[_unfinishedMessage.ParentId];

            // Assert
            Assert.That(resultFinishTime, Is.EqualTo(initialFinishTime));
        }

        [Test]
        public void RegisterMessagesShouldRegisterNewMessagesOnlyOnce()
        {
            // Arrange
            _messageRegistrator.RegisterMessages();

            var registeredMessages = _messageRegistrator.MessagesStartTimes;

            var initialFinishTime = registeredMessages[_unfinishedMessage.ParentId];

            // Act
            _messageRegistrator.RegisterMessages();

            var resultFinishTime = registeredMessages[_unfinishedMessage.ParentId];

            // Assert
            Assert.That(resultFinishTime, Is.EqualTo(initialFinishTime));
        }
    }
}
