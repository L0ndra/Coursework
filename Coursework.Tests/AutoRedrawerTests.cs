using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Coursework.Data.AutoRunners;
using Coursework.Data.Constants;
using Coursework.Gui.Drawers;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class AutoRedrawerTests
    {
        private Mock<IComponentDrawer> _componentDrawerMock;
        private IAutoRunner _autoRedrawer;

        [SetUp]
        public void Setup()
        {
            _componentDrawerMock = new Mock<IComponentDrawer>();

            _autoRedrawer = new AutoRedrawer(_componentDrawerMock.Object);

            _componentDrawerMock.Setup(c => c.UpdateComponents());
        }

        [Test]
        [Ignore("Test doesn't work")]
        public async Task RunShouldCallMethodUpdateAtLeastOnceAsync()
        {
            // Arrange
            // Act
            var dispatcher = Dispatcher.CurrentDispatcher;

            await dispatcher.BeginInvoke((Action)(async () =>
            {
                _autoRedrawer.Run();
                await Task.Delay(TimeSpan.FromMilliseconds(AllConstants.TimerInterval * 2));

                // Assert
                _componentDrawerMock.Verify(c => c.UpdateComponents(), Times.AtLeastOnce());
            }));
        }
    }
}
