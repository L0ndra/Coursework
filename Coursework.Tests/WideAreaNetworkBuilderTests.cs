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
    public class WideAreaNetworkBuilderTests
    {
        private Mock<INetworkBuilder> _simpleNetworkBuilderMock;
        private WideAreaNetworkBuilder _wideAreaNetworkBuilder;
        private int _numberOfMetropolitanNetworks;
        private uint _lastId;

        [SetUp]
        public void Setup()
        {
            _simpleNetworkBuilderMock = new Mock<INetworkBuilder>();

            _numberOfMetropolitanNetworks = 5;
            const int nodesCount = 1;

            _wideAreaNetworkBuilder = new WideAreaNetworkBuilder(_simpleNetworkBuilderMock.Object,
                _numberOfMetropolitanNetworks);

            _simpleNetworkBuilderMock.Setup(n => n.Build())
                .Returns(() => GenerateNetworkMockForTests(nodesCount).Object);
        }

        [Test]
        public void BuildShouldReturnCorrectNetworkWithSpecifiedNumberOfRegionalNetworks()
        {
            // Arrange

            // Act
            var result = _wideAreaNetworkBuilder.Build();

            // Assert
            var resultWithoutDublicates = result.Nodes
                .GroupBy(n => n.Id)
                .Select(n => n.Key);

            Assert.That(resultWithoutDublicates.Count(), Is.EqualTo(result.Nodes.Length));
            Assert.That(result.Channels.Count(c => c.ChannelType == ChannelType.Satellite), 
                Is.EqualTo(_numberOfMetropolitanNetworks));
        }

        private Mock<INetworkHandler> GenerateNetworkMockForTests(int count)
        {
            var networkMock = new Mock<INetworkHandler>();

            var nodes = new List<Node>();

            for (var i = 0; i < count; i++)
            {
                var node = new Node
                {
                    Id = _lastId,
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueue = new List<MessageQueueHandler>()
                };
                _lastId++;

                nodes.Add(node);
            }

            networkMock.Setup(n => n.Nodes)
                .Returns(nodes.ToArray());

            networkMock.Setup(n => n.Channels)
                .Returns(new Channel[0]);

            return networkMock;
        }
    }
}
