using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Exceptions;

namespace Coursework.Data
{
    public class Network : INetwork
    {
        private readonly IList<Node> _nodes;
        private readonly IList<Connection> _connections;
        public Node[] Nodes => _nodes.ToArray();
        public Connection[] Connections => _connections.ToArray();

        public Network()
        {
            _nodes = new List<Node>();
            _connections = new List<Connection>();
        }

        public void AddNode(Node node)
        {
            ThrowExceptionIfNodeExists(node.Id);

            _nodes.Add(node);
        }

        public void AddConnection(Connection connection)
        {
            ThrowExceptionIfConnectionExists(connection.StartNodeId, connection.EndNodeId);
            ThrowExceptionIfNodeNotExists(connection.StartNodeId);
            ThrowExceptionIfNodeNotExists(connection.EndNodeId);
            ThrowExceptionIfPriceIsIncorrect(connection.Price);
            ThrowExceptionIfErrorChanceIsIncorrect(connection.ErrorChance);

            _connections.Add(connection);
        }

        private void ThrowExceptionIfErrorChanceIsIncorrect(double errorChance)
        {
            if (errorChance < 0.0 || errorChance > 1.0)
            {
                throw new ConnectionException("Error chance is in incorrect range");
            }
        }

        private void ThrowExceptionIfPriceIsIncorrect(int price)
        {
            if (price <= 0)
            {
                throw new ConnectionException("Price is less or equal to 0");
            }
        }

        private void ThrowExceptionIfNodeNotExists(int nodeId)
        {
            if (_nodes.All(n => n.Id != nodeId))
            {
                throw new NodeException("Node isn't exists");
            }
        }

        private void ThrowExceptionIfNodeExists(int nodeId)
        {
            if (_nodes.Any(n => n.Id == nodeId))
            {
                throw new NodeException("Node is already exists");
            }
        }

        private void ThrowExceptionIfConnectionExists(int startNodeId, int endNodeId)
        {
            if (_connections.Any(c => c.StartNodeId == startNodeId && c.EndNodeId == endNodeId))
            {
                throw new ConnectionException("Connection is already exists");
            }
        }
    }
}
