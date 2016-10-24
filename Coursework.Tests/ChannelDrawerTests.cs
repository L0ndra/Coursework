using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Entities;
using Coursework.Gui.Drawers;
using Coursework.Gui.Dto;
using Coursework.Gui.Initializers;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ChannelDrawerTests
    {
        private Panel _panel;
        private Mock<INetworkHandler> _networkMock;
        private IComponentDrawer _channelDrawer;

        [SetUp]
        public void Setup()
        {
            _panel = new Canvas()
            {
                Width = 700,
                Height = 700
            };
            _networkMock = new Mock<INetworkHandler>();
            _channelDrawer = new ChannelDrawer(_networkMock.Object);

            const int nodesCount = 5;

            var nodes = Enumerable
                .Range(0, nodesCount)
                .Select(i => new Node { Id = (uint)i })
                .ToArray();

            _networkMock.Setup(n => n.Nodes)
                .Returns(nodes);

            var channels = new[]
            {
                new Channel
                {
                    Id = Guid.NewGuid(),
                    FirstNodeId = 0,
                    SecondNodeId = 1,
                },
                new Channel
                {
                    Id = Guid.NewGuid(),
                    FirstNodeId = 3,
                    SecondNodeId = 2
                }
            };

            _networkMock.Setup(n => n.Channels)
                .Returns(channels);
        }

        [Test]
        public void DrawComponentsShouldAddToPanelSpecifiedNumberOfLines()
        {
            // Arrange
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            CreateGridsForTests();

            // Act
            _channelDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(_panel.Children.OfType<Line>().Count, Is.EqualTo(_networkMock.Object.Channels.Length));
        }

        [Test]
        public void DrawComponentsShouldAddToPanelLinesOnce()
        {
            // Arrange
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            CreateGridsForTests();

            _channelDrawer.DrawComponents(_panel);

            // Act
            _channelDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(_panel.Children.OfType<Line>().Count, Is.EqualTo(_networkMock.Object.Channels.Length));
        }

        [Test]
        public void DrawComponentsShouldThrowExceptionIfPanelHasNoChildren()
        {
            // Arrange
            // Act
            TestDelegate testDelegate = () => _channelDrawer.DrawComponents(_panel);

            // Assert
            Assert.That(testDelegate, Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveCreatedElementsShouldRemoveAllCreatedChildren()
        {
            // Arrange
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            CreateGridsForTests();

            _channelDrawer.DrawComponents(_panel);

            // Act
            _channelDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.EqualTo(_networkMock.Object.Nodes.Length));
        }

        [Test]
        public void RemoveCreatedElementsShouldDoNothing()
        {
            // Arrange
            // Act
            _channelDrawer.RemoveCreatedElements();

            // Assert
            Assert.That(_panel.Children.Count, Is.Zero);
        }

        private void CreateGridsForTests()
        {
            for (var i = 0; i < _networkMock.Object.Nodes.Length; i++)
            {
                var element = new Grid
                {
                    Tag = new NodeDto
                    {
                        Id = (uint) i
                    }
                };

                Canvas.SetLeft(element, 10);
                Canvas.SetTop(element, 10);

                _panel.Children.Add(element);
            }
        }
    }
}
