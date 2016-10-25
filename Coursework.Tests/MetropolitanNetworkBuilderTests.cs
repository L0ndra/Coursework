using System.Collections.Generic;
using System.Linq;
using Coursework.Data;
using Coursework.Data.Builder;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class MetropolitanNetworkBuilderTests
    {
        private Mock<INetworkBuilder> _simpleNetworkBuilderMock;
        private Mock<INetworkHandler> _networkMock;
        private MetropolitanNetworkBuilder _metropolitanNetworkBuilder;

        [SetUp]
        public void Setup()
        {
            _simpleNetworkBuilderMock = new Mock<INetworkBuilder>();

            _networkMock = new Mock<INetworkHandler>();

            const int numberOfMetropolitanNetworks = 5;

            _metropolitanNetworkBuilder = new MetropolitanNetworkBuilder(_simpleNetworkBuilderMock.Object,
                numberOfMetropolitanNetworks);

            var nodes = new[]
            {
                new Node
                {
                    Id = 0,
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueue = new MessageQueueHandler()
                }
            };

            _networkMock.Setup(n => n.Nodes)
                .Returns(nodes);

            _networkMock.Setup(n => n.Channels)
                .Returns(new Channel[0]);
        }

        [Test]
        [Ignore("method not implemented")]
        public void BuildShouldReturnCorrectNetworkWithSpecifiedNumberOfRegionalNetworks()
        {
            // Arrange

            // Act
            var result = _metropolitanNetworkBuilder.Build();

            // Assert
            var resultWithoutDublicates = result.Nodes
                .GroupBy(n => n.Id)
                .Select(n => n.Key);

            Assert.That(resultWithoutDublicates.Count(), Is.EqualTo(result.Nodes.Length));
            Assert.IsTrue(result.Channels.Any(c => c.ChannelType == ChannelType.Satellite));
        }
    }
}
