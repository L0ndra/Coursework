using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using AutoMapper;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;
using Coursework.Gui.Drawers;
using Coursework.Gui.Initializers;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class NodeDrawerTests
    {
        private Mock<INetworkHandler> _networkMock;
        private IComponentDrawer _nodeDrawer;
        private Panel _panel;

        [SetUp]
        public void Setup()
        {
            _panel = new Canvas
            {
                Width = 700,
                Height = 700
            };
            _networkMock = new Mock<INetworkHandler>();
            _nodeDrawer = new NodeDrawer(_networkMock.Object);

            const int nodesCount = 5;

            var nodes = Enumerable
                .Range(0, nodesCount)
                .Select(i => new Node
                {
                    Id = (uint)i,
                    MessageQueueHandlers = new List<MessageQueueHandler>()
                })
                .ToArray();

            _networkMock.Setup(n => n.Nodes)
                .Returns(nodes);
        }

        [Test]
        public void DrawComponentsShouldAddToPanelSpecifiedNumberOfGrids()
        {
            // Arrange
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            // Act
            _nodeDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }

        [Test]
        public void DrawComponentsShouldAddToOneNodeOnce()
        {
            // Arrange
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            _nodeDrawer.DrawComponents(_panel);

            // Act
            _nodeDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }

        [Test]
        public void RemoveCreatedElementsShouldRemoveAllCreatedChildren()
        {
            // Arrange
            _nodeDrawer.DrawComponents(_panel);

            // Act
            _nodeDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }

        [Test]
        public void RemoveCreatedElementsShouldDoNothing()
        {
            // Arrange
            // Act
            _nodeDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }

        [Test]
        public void UpdateShouldNotThrowAnyException()
        {
            // Arrange
            // Act
            TestDelegate testDelegate = () => _nodeDrawer.UpdateComponents();

            // Assert
            Assert.That(testDelegate, Throws.Nothing);
        }
    }
}
