using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Builder;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkBuilderTests
    {
        private INetworkBuilder _networkBuilder;
        private Mock<INodeGenerator> _nodeGeneratorMock;
        private const int TimesToCreateNetwork = 1000;
        private readonly int[] _availablePrices = { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 };

        [SetUp]
        public void Setup()
        {
            _nodeGeneratorMock = new Mock<INodeGenerator>();
            var closureCounter = 0;

            _nodeGeneratorMock.Setup(n => n.GenerateNodes(It.IsAny<int>()))
                .Returns((int count) =>
                {
                    var result = Enumerable
                        .Range(closureCounter, count)
                        .Select(i => new Node
                        {
                            Id = (uint) i,
                            LinkedNodesId = new SortedSet<uint>(),
                            MessageQueueHandlers = new List<MessageQueueHandler>()
                        })
                        .ToArray();

                    closureCounter += count;

                    return result;
                }
                );
        }

        [Test]
        [TestCase((uint)5, 2.0, 0.9)]
        [TestCase((uint)20, 2.0, 0.9)]
        [TestCase((uint)20, 19.0, 19.0)]
        [TestCase((uint)10, 1.0, 0.9)]
        public void BuildShouldCreateCorrectNetworkSpecifiedNumberOfTimesWithSpecifiedParameters(uint nodeCount, double networkPower, double eps)
        {
            for (var i = 0; i < TimesToCreateNetwork; i++)
            {
                // Arrange
                _networkBuilder = new NetworkBuilder(_nodeGeneratorMock.Object, nodeCount, networkPower);

                // Act
                var network = _networkBuilder.Build();
                var currentPower = network.Channels.Length * 2 / (double)nodeCount;
                var isAllPricesIsAvailable = network.Channels
                    .Select(c => c.Price)
                    .All(p => _availablePrices.Contains(p));

                // Assert
                Assert.That(network.Nodes.Length, Is.EqualTo(nodeCount));
                Assert.That(network.Channels.Length, Is.GreaterThan(0));
                Assert.IsTrue(isAllPricesIsAvailable);
                Assert.That(Math.Abs(currentPower - networkPower), Is.LessThanOrEqualTo(eps));

                LogResult(i, networkPower, currentPower);
            }
        }

        [Test]
        [TestCase((uint)5, 2.0)]
        public void BuildShouldTwoNetworksWithDifferentIds(uint nodeCount, double networkPower)
        {
            // Arrange
            _networkBuilder = new NetworkBuilder(_nodeGeneratorMock.Object, nodeCount, networkPower);
            var firstNetwork = _networkBuilder.Build();

            // Act
            var result = _networkBuilder.Build();

            // Assert
            foreach (var node in result.Nodes)
            {
                Assert.IsFalse(firstNetwork.Nodes.Any(n => n.Id == node.Id));
            }
        }

        [Test]
        [TestCase((uint) 5, 2.0)]
        public void BuildShouldReturnNetworkWithOneCentralMachine(uint nodeCount, double networkPower)
        {
            // Arrange
            // Act
            var result = _networkBuilder.Build();

            // Assert
            Assert.That(result.Nodes.Count(n => n.NodeType == NodeType.CentralMachine), Is.EqualTo(1));
        }

        private static void LogResult(int testNumber, double networkPower, double currentPower)
        {
            Console.WriteLine($"Test {testNumber}: ");
            Console.WriteLine($"\tcurrent power = {currentPower};");
            Console.WriteLine($"\tspecified power = {networkPower}");
            Console.WriteLine($"\tdifference ABS = {Math.Abs(networkPower - currentPower)}");
        }
    }
}
