using System;
using System.Collections.Generic;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessagesStatisticCounterTests
    {
        private Mock<IMessageRegistrator> _messageRegistratorMock;
        private Mock<IMessageRepository> _messageRepoMock;
        private IMessagesStatisticCounter _messagesStatisticCounter;
        private Message _finishedMessage;
        private Message _unfinishedMessage;
        private MessagesStatistic _expectedStatistic;

        [SetUp]
        public void Setup()
        {
            _messageRegistratorMock = new Mock<IMessageRegistrator>();
            _messageRepoMock = new Mock<IMessageRepository>();

            _messagesStatisticCounter = new MessagesStatisticCounter(_messageRegistratorMock.Object,
                _messageRepoMock.Object);

            _finishedMessage = new Message
            {
                ParentId = Guid.NewGuid(),
                IsReceived = true,
                DataSize = 5,
                ServiceSize = 2,
                MessageType = MessageType.MatrixUpdateMessage
            };

            _unfinishedMessage = new Message
            {
                ParentId = Guid.NewGuid(),
                DataSize = 5,
                ServiceSize = 2
            };

            _messageRegistratorMock.Setup(m => m.MessagesStartTimes)
                .Returns(new Dictionary<Guid, long>
                {
                    [_finishedMessage.ParentId] = 0,
                    [_unfinishedMessage.ParentId] = 1,
                });

            _messageRegistratorMock.Setup(m => m.MessagesEndTimes)
                .Returns(new Dictionary<Guid, long> { [_finishedMessage.ParentId] = 3 });

            _messageRepoMock.Setup(m => m.GetAllMessages(It.IsAny<uint?>(), It.IsAny<MessageFiltrationMode>()))
                .Returns(new[] {_finishedMessage, _unfinishedMessage});

            const int avarageDeliveryTime = 3;

            _expectedStatistic = new MessagesStatistic
            {
                MessagesCount = 2,
                GeneralMessagesCount = 1,
                AvarageDeliveryTime = avarageDeliveryTime,
                GeneralMessagesReceivedCount = 0,
                ReceivedMessagesCount = 1,
                TotalReceivedDataSize = _finishedMessage.DataSize,
                TotalReceivedMessagesSize = _finishedMessage.Size
            };
        }

        [Test]
        public void CountShouldReturnCorrectMessagesStatistic()
        {
            // Arrange
            // Act
            var actualStatistic = _messagesStatisticCounter.Count();

            // Assert
            Assert.That(actualStatistic.MessagesCount, Is.EqualTo(_expectedStatistic.MessagesCount));
            Assert.That(actualStatistic.GeneralMessagesCount, Is.EqualTo(_expectedStatistic.GeneralMessagesCount));
            Assert.That(actualStatistic.AvarageDeliveryTime, Is.EqualTo(_expectedStatistic.AvarageDeliveryTime));
            Assert.That(actualStatistic.GeneralMessagesReceivedCount, Is.EqualTo(_expectedStatistic.GeneralMessagesReceivedCount));
            Assert.That(actualStatistic.ReceivedMessagesCount, Is.EqualTo(_expectedStatistic.ReceivedMessagesCount));
            Assert.That(actualStatistic.TotalReceivedDataSize, Is.EqualTo(_expectedStatistic.TotalReceivedDataSize));
            Assert.That(actualStatistic.TotalReceivedMessagesSize, 
                Is.EqualTo(_expectedStatistic.TotalReceivedMessagesSize));
        }

        [Test]
        public void CountShouldReturnCorrectMessagesStatistic2()
        {
            // Arrange
            _unfinishedMessage.IsReceived = true;

            _messageRegistratorMock.Setup(m => m.MessagesEndTimes)
                .Returns(new Dictionary<Guid, long>
                {
                    [_finishedMessage.ParentId] = 3,
                    [_unfinishedMessage.ParentId] = 4,
                });

            // Act
            var actualStatistic = _messagesStatisticCounter.Count();

            // Assert
            Assert.That(actualStatistic.MessagesCount, Is.EqualTo(_expectedStatistic.MessagesCount));
            Assert.That(actualStatistic.GeneralMessagesCount, Is.EqualTo(_expectedStatistic.GeneralMessagesCount));
            Assert.That(actualStatistic.AvarageDeliveryTime, Is.EqualTo(_expectedStatistic.AvarageDeliveryTime));
            Assert.That(actualStatistic.GeneralMessagesReceivedCount, Is.EqualTo(_expectedStatistic.GeneralMessagesReceivedCount + 1));
            Assert.That(actualStatistic.ReceivedMessagesCount, Is.EqualTo(_expectedStatistic.ReceivedMessagesCount + 1));
            Assert.That(actualStatistic.TotalReceivedDataSize, Is.EqualTo(_expectedStatistic.TotalReceivedDataSize 
                + _unfinishedMessage.DataSize));
            Assert.That(actualStatistic.TotalReceivedMessagesSize,
                Is.EqualTo(_expectedStatistic.TotalReceivedMessagesSize + _unfinishedMessage.Size));
        }
    }
}
