using System.Linq;
using Coursework.Data;
using Coursework.Data.Builder;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkTests
    {
        private INetworkHandler _network;
        private Channel _channel;
        private Node _node1;
        private Node _node2;

        [SetUp]
        public void Setup()
        {
            _network = new Network();

            _node1 = NodeGenerator.GenerateNodes(0, 1).First();

            _node2 = NodeGenerator.GenerateNodes(1, 1).First();

            _channel = new Channel
            {
                SecondNodeId = _node2.Id,
                FirstNodeId = _node1.Id,
                Price = AllConstants.AllPrices.ElementAt(0),
                ChannelType = ChannelType.Ground,
                ErrorChance = 0.6,
                ConnectionType = ConnectionType.Duplex
            };
        }

        [Test]
        public void AddNodeShouldAddNodeWithCorrectId()
        {
            // Arrange
            // Act
            _network.AddNode(_node1);

            // Assert
            Assert.That(_network.Nodes.Any(n => n.Id == _node1.Id), Is.True);
        }

        [Test]
        public void AddNodeShouldThrowExceptionIfNodeExists()
        {
            // Arrange
            _network.AddNode(_node1);

            // Act
            TestDelegate testDelegate = () => _network.AddNode(_node1);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddChannelShouldAddChannelBetweenTwoNodes()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);

            // Act
            _network.AddChannel(_channel);

            // Assert
            Assert.That(_network.Channels.Contains(_channel), Is.True);
            Assert.That(_node1.LinkedNodesId.Contains(_node2.Id), Is.True);
            Assert.That(_node2.LinkedNodesId.Contains(_node1.Id), Is.True);
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfStartNodeNotExists()
        {
            // Arrange
            _channel.FirstNodeId = (uint)_network.Nodes.Length + 1;

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfEndNodeNotExists()
        {
            // Arrange
            _channel.SecondNodeId = (uint)_network.Nodes.Length + 1;

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfErrorChanceIsIncorrect()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _channel.ErrorChance = 1.2;

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ChannelException)));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfPriceIsNegative()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _channel.Price = -1;

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ChannelException)));
        }

        [Test]
        public void GetChannelShouldReturnChannelIfChannelFromFirstToSecondNodeExists()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            // Act
            var result = _network.GetChannel(_node1.Id, _node2.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.IsTrue(result.FirstNodeId == _node1.Id || result.FirstNodeId == _node2.Id);
            Assert.IsTrue(result.SecondNodeId == _node1.Id || result.SecondNodeId == _node2.Id);
        }

        [Test]
        public void GetChannelShouldReturnChannelIfChannelFromSecondToFirstNodeExists()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            // Act
            var result = _network.GetChannel(_node2.Id, _node1.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.IsTrue(result.FirstNodeId == _node1.Id || result.FirstNodeId == _node2.Id);
            Assert.IsTrue(result.SecondNodeId == _node1.Id || result.SecondNodeId == _node2.Id);
        }

        [Test]
        public void GetChannelShouldReturnNullIfChannelNotExists()
        {
            // Arrange
            // Act
            var result = _network.GetChannel(_node2.Id, _node1.Id);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetLinkedNodeWithLinkPriceShouldReturnDictionaryWithNodesAndPrices()
        {
            // Arrange 
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            // Act
            var result = _network.GetLinkedNodeIdsWithLinkPrice(_channel.FirstNodeId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[_channel.SecondNodeId], Is.EqualTo(_channel.Price));
        }

        [Test]
        public void UpdateChannelShouldChangeExistedChannel()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            _channel.Price = AllConstants.AllPrices.ElementAt(2);
            _channel.ChannelType = ChannelType.Satellite;

            // Act
            _network.UpdateChannel(_channel);
            var updatedChannel = _network.GetChannel(_channel.FirstNodeId, _channel.SecondNodeId);

            // Assert
            Assert.That(updatedChannel.Price, Is.EqualTo(AllConstants.AllPrices.ElementAt(2)));
            Assert.That(updatedChannel.ChannelType, Is.EqualTo(ChannelType.Satellite));
            Assert.That(updatedChannel.ConnectionType, Is.EqualTo(_channel.ConnectionType));
        }

        [Test]
        public void UpdateChannelShouldThrowExceptionIfChannelNotExists()
        {
            // Arrange
            // Act
            TestDelegate testDelegate = () => _network.UpdateChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }
    }
}
