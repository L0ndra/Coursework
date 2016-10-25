using System.Collections.Generic;
using System.Linq;
using Coursework.Data;
using Coursework.Data.Builder;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using Coursework.Data.MessageServices;
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

            _node1 = new Node
            {
                Id = 0,
                LinkedNodesId = new SortedSet<uint>(),
                MessageQueue = new List<MessageQueueHandler>()
            };
            _node2 = new Node
            {
                Id = 1,
                LinkedNodesId = new SortedSet<uint>(),
                MessageQueue = new List<MessageQueueHandler>()
            };

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
            Assert.IsTrue(_network.Channels.Contains(_channel));
            Assert.IsTrue(_node1.LinkedNodesId.Contains(_node2.Id));
            Assert.IsTrue(_node2.LinkedNodesId.Contains(_node1.Id));
            Assert.IsTrue(_node1.LinkedNodesId.Contains(_node2.Id));
            Assert.IsTrue(_node2.LinkedNodesId.Contains(_node1.Id));
        }

        [Test]
        public void AddChannelShouldCreateTwoMessageQueuesInNodes()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);

            // Act
            _network.AddChannel(_channel);

            // Assert
            Assert.That(_node1.MessageQueue.Any(m => m.ChannelId == _channel.Id));
            Assert.That(_node2.MessageQueue.Any(m => m.ChannelId == _channel.Id));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfStartNodeNotExists()
        {
            // Arrange
            _channel.FirstNodeId = uint.MaxValue;

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfEndNodeNotExists()
        {
            // Arrange
            _channel.SecondNodeId = uint.MaxValue;

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfChannelWithSameIdExistsInNetwork()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            // Act
            TestDelegate testDelegate = () => _network.AddChannel(_channel);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ChannelException)));
        }

        [Test]
        public void AddChannelShouldThrowExceptionIfErrorChanceIsIncorrect()
        {
            // Arrange
            CreateTwoNodesForTests();
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
            CreateTwoNodesForTests();
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
            CreateTwoNodesForTests();
            CreateChannelForTests();

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
            CreateTwoNodesForTests();
            CreateChannelForTests();

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
        public void GetChannelsShouldReturnAllChannelsLinkedWithCurrentNode()
        {
            // Arrange 
            CreateTwoNodesForTests();
            CreateChannelForTests();

            // Act
            var result = _network.GetChannels(_channel.FirstNodeId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.IsTrue(result.Any(c => c.SecondNodeId == _channel.SecondNodeId || c.FirstNodeId == _channel.SecondNodeId));
        }

        [Test]
        public void UpdateChannelShouldChangeExistedChannel()
        {
            // Arrange
            CreateTwoNodesForTests();
            CreateChannelForTests();

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

        private void CreateChannelForTests()
        {
            _network.AddChannel(_channel);
        }

        private void CreateTwoNodesForTests()
        {
            _network.AddNode(_node1);
            _network.AddNode(_node2);
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

        [Test]
        public void RemoveChannelShouldDropOneChannelIfItExists()
        {
            // Arrange
            CreateTwoNodesForTests();
            CreateChannelForTests();
            var initialChannelCount = _network.Channels.Length;

            // Act
            _network.RemoveChannel(_node1.Id, _node2.Id);
            var resultChannelCount = _network.Channels.Length;

            // Assert
            Assert.That(resultChannelCount, Is.EqualTo(initialChannelCount - 1));
        }

        [Test]
        public void RemoveChannelShouldDropOneChannelIfItExistsAndIdsSwaped()
        {
            // Arrange
            CreateTwoNodesForTests();
            CreateChannelForTests();
            var initialChannelCount = _network.Channels.Length;

            // Act
            _network.RemoveChannel(_node2.Id, _node1.Id);
            var resultChannelCount = _network.Channels.Length;

            // Assert
            Assert.That(resultChannelCount, Is.EqualTo(initialChannelCount - 1));
        }

        [Test]
        public void RemoveChannelShouldDoNothingIfChannelNotExists()
        {
            // Arrange
            CreateTwoNodesForTests();
            var initialChannelCount = _network.Channels.Length;

            // Act
            _network.RemoveChannel(_node2.Id, _node1.Id);
            var resultChannelCount = _network.Channels.Length;

            // Assert
            Assert.That(resultChannelCount, Is.EqualTo(initialChannelCount));
        }

        [Test]
        public void RemoveNodeShouldRemoveNodeWithAllLinkedChannels()
        {
            // Arrange
            CreateTwoNodesForTests();
            CreateChannelForTests();
            var initialNodeCount = _network.Nodes.Length;
            var initialChannelCount = _network.Nodes.Length;

            // Act
            _network.RemoveNode(_node1.Id);
            var resultNodeCount = _network.Nodes.Length;
            var resultChannelCount = _network.Nodes.Length;

            // Assert
            Assert.That(resultNodeCount, Is.EqualTo(initialNodeCount - 1));
            Assert.That(resultChannelCount, Is.EqualTo(initialChannelCount - 1));
        }

        [Test]
        public void RemoveNodeShouldDoNothingIfNodeNotExists()
        {
            // Arrange
            var initialNodeCount = _network.Nodes.Length;
            var initialChannelCount = _network.Nodes.Length;

            // Act
            _network.RemoveNode(_node1.Id);
            var resultNodeCount = _network.Nodes.Length;
            var resultChannelCount = _network.Nodes.Length;

            // Assert
            Assert.That(resultNodeCount, Is.EqualTo(initialNodeCount));
            Assert.That(resultChannelCount, Is.EqualTo(initialChannelCount));
        }
    }
}
