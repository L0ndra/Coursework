using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;
using Coursework.Data;
using Coursework.Data.Drawers;
using Coursework.Data.Entities;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class NetworkDrawerTests
    {
        private Mock<INetwork> _networkMock;
        private INetworkDrawer _networkDrawer;
        private Mock<IComponentDrawer> _nodeDrawerMock;
        private Mock<IComponentDrawer> _channelDrawerMock;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetwork>();

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
        public void DrawNetworkShouldReturnCanvasWithCorrectUiElements()
        {
            // Arrange
            // Act
            var result = _networkDrawer.DrawNetwork(_networkMock.Object, 700, 700);

            // Assert
            _nodeDrawerMock.Verify(n => n.DrawComponents(_networkMock.Object, It.IsAny<Panel>()), Times.Once());
            _channelDrawerMock.Verify(n => n.DrawComponents(_networkMock.Object, It.IsAny<Panel>()), Times.Once());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void DrawNetworkShouldThrowExceptionIfWidthOrHeightIsNegative()
        {
            // Arrange
            // Act
            TestDelegate testDelegate = () => _networkDrawer.DrawNetwork(_networkMock.Object, -700, 700);

            // Assert
            Assert.That(testDelegate, Throws.ArgumentException);
        }
    }
}
