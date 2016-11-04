using Coursework.Data.Constants;
using Coursework.Data.MessageServices;
using Coursework.Gui.Background;
using Coursework.Gui.Drawers;
using Coursework.Gui.MessageService;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class BackgroundWorkerTests
    {
        private Mock<IMessageExchanger> _messageExchangerMock;
        private Mock<IMessageGenerator> _messageGeneratorMock;
        private Mock<IComponentDrawer> _networkDrawerMock;
        private Mock<IMessageCreator> _messageCreatorMock;
        private BackgroundWorker _backgroundWorker;
        private Mock<IMessageViewUpdater> _messageViewUpdaterMock;

        [SetUp]
        public void Setup()
        {
            _messageExchangerMock = new Mock<IMessageExchanger>();
            _messageGeneratorMock = new Mock<IMessageGenerator>();
            _networkDrawerMock = new Mock<IComponentDrawer>();
            _messageCreatorMock = new Mock<IMessageCreator>();
            _messageViewUpdaterMock = new Mock<IMessageViewUpdater>();

            _backgroundWorker = new BackgroundWorker(_messageExchangerMock.Object, _messageGeneratorMock.Object,
                _networkDrawerMock.Object, _messageCreatorMock.Object, _messageViewUpdaterMock.Object,
                AllConstants.UpdateTablePeriod);
        }

        [Test]
        public void RunShouldStartTimer()
        {
            // Arrange
            // Act
            _backgroundWorker.Run();

            // Assert
            Assert.IsTrue(_backgroundWorker.IsActive);
        }

        [Test]
        public void PauseShouldPauseTimer()
        {
            // Arrange
            _backgroundWorker.Run();

            // Act
            _backgroundWorker.Pause();

            // Assert
            Assert.IsFalse(_backgroundWorker.IsActive);
        }

        [Test]
        public void StopShouldStopTimer()
        {
            // Arrange
            _backgroundWorker.Run();

            // Act
            _backgroundWorker.Stop();

            // Assert
            Assert.IsFalse(_backgroundWorker.IsActive);
        }

        [Test]
        public void ResumeShouldResumeTimer()
        {
            // Arrange
            _backgroundWorker.Run();
            _backgroundWorker.Pause();

            // Act
            _backgroundWorker.Resume();

            // Assert
            Assert.IsTrue(_backgroundWorker.IsActive);
        }

        [Test]
        public void TicksShouldResetAfterTimerInitialize()
        {
            // Arrange
            _backgroundWorker = new BackgroundWorker(_messageExchangerMock.Object, _messageGeneratorMock.Object,
                _networkDrawerMock.Object, _messageCreatorMock.Object, _messageViewUpdaterMock.Object,
                AllConstants.UpdateTablePeriod);

            // Act
            var result = _backgroundWorker.Ticks;

            // Assert
            Assert.That(result, Is.Zero);
        }
    }
}
