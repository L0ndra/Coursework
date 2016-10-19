using System;
using System.Linq;
using Coursework.Data.Builder;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkBuilderTests
    {
        private INetworkBuilder _networkBuilder;
        private uint _nodeCount;
        private double _networkPower;

        [SetUp]
        public void Setup()
        {
            _nodeCount = 5;
            _networkPower = 2.5;
            _networkBuilder = new NetworkBuilder(_nodeCount, _networkPower);
        }

        [Test]
        public void BuildShouldCreateNetworkWithConcreteNumberOfNodesWithLinks()
        {
            // Arrange
            _networkBuilder = new NetworkBuilder(_nodeCount, _networkPower);

            // Act
            var network = _networkBuilder.Build();

            // Assert
            var group = network.Channels
                .GroupBy(c => c.FirstNodeId)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToArray();

            Assert.That(network.Nodes.Length, Is.EqualTo(_nodeCount));
            Assert.That(network.Channels.Length, Is.GreaterThan(0));
            Assert.That(Math.Abs(group.Sum(g => g.Count) / (double)_nodeCount - _networkPower),
                Is.LessThanOrEqualTo(_nodeCount / 2.0));
        }
    }
}
