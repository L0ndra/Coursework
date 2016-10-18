using System.Linq;
using Coursework.Data;
using Coursework.Data.Exceptions;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkTests
    {
        private Network _network;
        private Connection _connection;
        private Node _node1;
        private Node _node2;

        [SetUp]
        public void Setup()
        {
            _network = new Network();

            _connection = new Connection
            {
                EndNodeId = 0,
                StartNodeId = 1,
                Price = 5,
                ChannelType = ChannelType.Ground,
                ErrorChance = 0.6,
                NodeLinkType = ConnectionType.Duplex
            };

            _node1 = new Node
            {
                Id = 0
            };

            _node2 = new Node
            {
                Id = 1
            };
        }

        [Test]
        public void AddNodeShouldAddNodeWithIdZero()
        {
            // Arrange
            // Act
            _network.AddNode(_node1);

            // Assert
            Assert.That(_network.Nodes.Any(n => n.Id == _node1.Id), Is.True);
        }

        [Test]
        public void AddNodeShouldAddNodeWithIdOne()
        {
            // Arrange
            _network.AddNode(_node1);

            // Act
            _network.AddNode(_node2);

            // Assert
            Assert.That(_network.Nodes.Any(n => n.Id == _node2.Id), Is.True);

        }

        [Test]
        public void AddConnectionShouldAddConnectionBetweenTwoNodes()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);

            // Act
            _network.AddConnection(_connection);

            // Assert
            Assert.That(_network.Connections.Contains(_connection), Is.True);
        }

        [Test]
        public void AddConnectionShouldThrowExceptionIfStartNodeNotExists()
        {
            // Arrange
            _connection.StartNodeId = _network.Nodes.Length + 1;

            // Act
            TestDelegate testDelegate = () => _network.AddConnection(_connection);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddConnectionShouldThrowExceptionIfEndNodeNotExists()
        {
            // Arrange
            _connection.EndNodeId = _network.Nodes.Length + 1;

            // Act
            TestDelegate testDelegate = () => _network.AddConnection(_connection);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NodeException)));
        }

        [Test]
        public void AddConnectionShouldThrowExceptionIfErrorChanceIsIncorrect()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _connection.ErrorChance = 1.2;

            // Act
            TestDelegate testDelegate = () => _network.AddConnection(_connection);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ConnectionException)));
        }

        [Test]
        public void AddConnectionShouldThrowExceptionIfPriceIsNegative()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _connection.Price = -1;

            // Act
            TestDelegate testDelegate = () => _network.AddConnection(_connection);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ConnectionException)));
        }

        [Test]
        public void AddConnectionShouldThrowExceptionIfConnectionExists()
        {
            // Arrange
            _network.AddNode(_node1);
            _network.AddNode(_node2);
            _network.AddConnection(_connection);

            // Act
            TestDelegate testDelegate = () => _network.AddConnection(_connection);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(ConnectionException)));
        }
    }
}
