using Coursework.Data.AutoRunners;
using Coursework.Data.MessageServices;
using Coursework.Gui.Background;
using Coursework.Gui.Drawers;
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
        private IBackgroundWorker _backgroundWorker;

        [SetUp]
        public void Setup()
        {
            _messageExchangerMock = new Mock<IMessageExchanger>();
            _messageGeneratorMock = new Mock<IMessageGenerator>();
            _networkDrawerMock = new Mock<IComponentDrawer>();

            _backgroundWorker = new BackgroundWorker(_messageExchangerMock.Object, _messageGeneratorMock.Object,
                _networkDrawerMock.Object);
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
    }
}
