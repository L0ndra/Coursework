using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Entities;
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
            _nodeDrawer = new NodeDrawer();

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
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            // Act
            _nodeDrawer.DrawComponents(_panel, _networkMock.Object);

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }
    }
}
