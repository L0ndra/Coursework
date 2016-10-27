using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;
using Coursework.Data.Services;
using Moq;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class WideAreaNetworkServiceTests
    {
        private IWideAreaNetworkService _wideAreaNetworkService;
        private Mock<INetworkHandler> _networkMock;
        private int _nodesCount;

        [SetUp]
        public void Setup()
        {
            _networkMock = new Mock<INetworkHandler>();

            _wideAreaNetworkService = new WideAreaNetworkService(_networkMock.Object);

            _nodesCount = 5;

            var nodes = Enumerable
                .Range(0, _nodesCount)
                .Select(i => new Node { Id = (uint)i, LinkedNodesId = new SortedSet<uint>() })
                .ToArray();

            _networkMock.Setup(n => n.Nodes)
                .Returns(nodes);

            _networkMock.Setup(n => n.GetNodeById(It.IsAny<uint>()))
                .Returns((uint nodeId) => nodes.FirstOrDefault(n => n.Id == nodeId));
        }

        [Test]
        public void GetNodesInOneMetropolitanNetworkShouldReturnAllGroups()
        {
            // Arrange
            // Act
            var result = _wideAreaNetworkService.GetNodesInOneMetropolitanNetwork();

            // Assert
            Assert.That(result.Length, Is.EqualTo(_nodesCount));
        }
    }
}
