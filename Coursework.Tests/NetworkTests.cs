using System.Linq;
using Coursework.Data;
using Coursework.Data.Exceptions;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkTests
    {
        private INetwork _network;
        private Channel _channel;
        private Node _node1;
        private Node _node2;

        [SetUp]
        public void Setup()
        {
            _network = new Network();

            _node1 = new Node
            {
                Id = 0
            };

            _node2 = new Node
            {
                Id = 1
            };

            _channel = new Channel
            {
                SecondNodeId = _node2.Id,
                FirstNodeId = _node1.Id,
                Price = 5,
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
        public void IsChannelExistsShouldReturnTrueIfChannelFromFirstToSecondNodeExists()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            // Act
            var result = _network.IsChannelExists(_node1.Id, _node2.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsChannelExistsShouldReturnTrueIfChannelFromSecondToFirstNodeExists()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddChannel(_channel);

            // Act
            var result = _network.IsChannelExists(_node2.Id, _node1.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsChannelExistsShouldReturnFalseIfChannelNotExists()
        {
            // Arrange
            // Act
            var result = _network.IsChannelExists(_node2.Id, _node1.Id);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
