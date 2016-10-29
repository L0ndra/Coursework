using System;
using System.Threading.Tasks;
using Coursework.Data.AutoRunners;
using Coursework.Data.Constants;
using Coursework.Data.MessageServices;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MessageExchangerRunnerTests
    {
        private Mock<IMessageExchanger> _messageExchangerMock;
        private IAutoRunner _messageExchangerRunner;

        [SetUp]
        public void Setup()
        {
            _messageExchangerMock = new Mock<IMessageExchanger>();

            _messageExchangerRunner = new MessageExchangerRunner(_messageExchangerMock.Object);
        }

        [Test]
        public async Task RunShouldCallHandleMessageAtLeastOnceAsync()
        {
            // Arrange
            // Act
            _messageExchangerRunner.Run();

            // Assert
            await Task.Delay(TimeSpan.FromMilliseconds(AllConstants.TimerInterval * 2));
            _messageExchangerMock.Verify(m => m.HandleMessagesOnce(), Times.AtLeastOnce());
        }
    }
}
