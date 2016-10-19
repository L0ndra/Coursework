using System;
using System.Linq;
using System.Threading;
using System.Windows;
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
    public class ChannelDrawerTests
    {
        private Panel _panel;
        private Mock<INetwork> _networkMock;
        private IComponentDrawer _channelDrawer;

        [SetUp]
        public void Setup()
        {
            _panel = new Canvas()
            {
                Width = 700,
                Height = 700
            };
            _networkMock = new Mock<INetwork>();
            _channelDrawer = new ChannelDrawer();

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
                    FirstNodeId = 0,
                    SecondNodeId = 1,
                },
                new Channel
                {
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
            for (var i = 0; i < _networkMock.Object.Nodes.Length; i++)
            {
                var element = new UIElement();

                Canvas.SetLeft(element, 10);
                Canvas.SetTop(element, 10);

                _panel.Children.Add(element);
            }

            // Act
            _channelDrawer.DrawComponents(_networkMock.Object, _panel);

            // Assert
            Assert.That(_panel.Children.OfType<Line>().Count, Is.EqualTo(_networkMock.Object.Channels.Length));
        }

        [Test]
        public void DrawComponentsShouldThrowExceptionIfPanelHasNoChildren()
        {
            // Arrange
            // Act
            TestDelegate testDelegate = () => _channelDrawer.DrawComponents(_networkMock.Object, _panel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ArgumentOutOfRangeException)));
        }
    }
}
