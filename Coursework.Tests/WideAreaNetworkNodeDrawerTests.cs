﻿using System.Linq;
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
    [Ignore("Class methods aren't implemented")]
    public class WideAreaNetworkNodeDrawerTests
    {
        private Mock<INetworkHandler> _networkMock;
        private IComponentDrawer _wideAreaNetworkNodeDrawer;
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
            _wideAreaNetworkNodeDrawer = new WideAreaNetworkNodeDrawer(_networkMock.Object);

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
            _wideAreaNetworkNodeDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }

        [Test]
        public void DrawComponentsShouldAddToOneNodeOnce()
        {
            // Arrange
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            _wideAreaNetworkNodeDrawer.DrawComponents(_panel);

            // Act
            _wideAreaNetworkNodeDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }

        [Test]
        public void RemoveCreatedElementsShouldRemoveAllCreatedChildren()
        {
            // Arrange
            _wideAreaNetworkNodeDrawer.DrawComponents(_panel);

            // Act
            _wideAreaNetworkNodeDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }

        [Test]
        public void RemoveCreatedElementsShouldDoNothing()
        {
            // Arrange
            // Act
            _wideAreaNetworkNodeDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }
    }
}
