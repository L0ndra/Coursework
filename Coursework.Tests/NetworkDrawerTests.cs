using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;
using Coursework.Gui.Drawers;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class NetworkDrawerTests
    {
        private Panel _panel;
        private Mock<INetworkHandler> _networkMock;
        private IComponentDrawer _networkDrawer;
        private Mock<IComponentDrawer> _nodeDrawerMock;
        private Mock<IComponentDrawer> _channelDrawerMock;

        [SetUp]
        public void Setup()
        {
            _panel = new Canvas()
            {
                Width = 700,
                Height = 700
            };

            _networkMock = new Mock<INetworkHandler>();

            _nodeDrawerMock = new Mock<IComponentDrawer>();
            _channelDrawerMock = new Mock<IComponentDrawer>();

            _networkDrawer = new NetworkDrawer(_nodeDrawerMock.Object, _channelDrawerMock.Object);

            const int nodeCount = 5;
            var nodes = Enumerable
                .Range(0, nodeCount)
                .Select(n => new Node { Id = (uint)n })
                .ToArray();

            _networkMock.Setup(n => n.Nodes)
                .Returns(nodes);
        }

        [Test]
        public void DrawComponentsShouldReturnCanvasWithCorrectUiElements()
        {
            // Arrange
            // Act
            _networkDrawer.DrawComponents(_panel);

            // Assert
            _nodeDrawerMock.Verify(n => n.DrawComponents(It.IsAny<Panel>()), Times.Once());
            _channelDrawerMock.Verify(n => n.DrawComponents(It.IsAny<Panel>()), Times.Once());
            Assert.That(_panel.Children.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveCreatedElementsShouldRemoveAllCreatedChildren()
        {
            // Arrange
            _networkDrawer.DrawComponents(_panel);

            // Act
            _networkDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }

        [Test]
        public void RemoveCreatedElementsShouldDoNothing()
        {
            // Arrange
            // Act
            _networkDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }
    }
}
