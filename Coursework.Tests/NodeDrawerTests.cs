using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Coursework.Data;
using Coursework.Data.Drawers;
using Coursework.Data.Entities;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class NodeDrawerTests
    {
        private Mock<INetwork> _networkMock;
        private IComponentDrawer _nodeDrawer;
        private Panel _panel;

        [SetUp]
        public void Setup()
        {
            _panel = new Canvas()
            {
                Width = 700,
                Height = 700
            };
            _networkMock = new Mock<INetwork>();
            var random = new Random((int)(DateTime.Now.Ticks & 0xFFFF));
            _nodeDrawer = new NodeDrawer(random);

            const int nodesCount = 5;

            var nodes = Enumerable
                .Range(0, nodesCount)
                .Select(i => new Node { Id = (uint)i })
                .ToArray();

            _networkMock.Setup(n => n.Nodes)
                .Returns(nodes);
        }

        [Test]
        public void DrawComponentsShouldAddToPanelSpecifiedNumberOfGrids()
        {
            // Arrange
            // Act
            _nodeDrawer.DrawComponents(_networkMock.Object, _panel);

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }
    }
}
