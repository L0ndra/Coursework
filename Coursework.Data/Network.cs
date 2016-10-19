using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;

namespace Coursework.Data
{
    public class Network : INetwork
    {
        private readonly IList<Node> _nodes;
        private readonly IList<Channel> _channels;
        public Node[] Nodes => _nodes.ToArray();
        public Channel[] Channels => _channels.ToArray();

        public Network()
        {
            _nodes = new List<Node>();
            _channels = new List<Channel>();
        }

        public void AddNode(Node node)
        {
            ThrowExceptionIfNodeCannotBeCreated(node);

            _nodes.Add(node);
        }

        public void AddChannel(Channel channel)
        {
            ThrowExceptionIfChannelCannotBeCreated(channel);

            _channels.Add(channel);
        }

        public bool IsChannelExists(uint firstNodeId, uint secondNodeId)
        {
            return Channels.Any(c => c.FirstNodeId == firstNodeId && c.SecondNodeId == secondNodeId
                                     || c.FirstNodeId == secondNodeId && c.SecondNodeId == firstNodeId);
        }

        private void ThrowExceptionIfChannelCannotBeCreated(Channel channel)
        {
            ThrowExceptionIfNodeNotExists(channel.FirstNodeId);
            ThrowExceptionIfNodeNotExists(channel.SecondNodeId);
            ThrowExceptionIfPriceIsIncorrect(channel.Price);
            ThrowExceptionIfErrorChanceIsIncorrect(channel.ErrorChance);
        }

        private void ThrowExceptionIfNodeCannotBeCreated(Node node)
        {
            ThrowExceptionIfNodeExists(node.Id);
        }

        private void ThrowExceptionIfErrorChanceIsIncorrect(double errorChance)
        {
            if (errorChance < 0.0 || errorChance > 1.0)
            {
                throw new ChannelException("Error chance is in incorrect range");
            }
        }

        private void ThrowExceptionIfPriceIsIncorrect(int price)
        {
            if (price <= 0)
            {
                throw new ChannelException("Price is less or equal to 0");
            }
        }

        private void ThrowExceptionIfNodeNotExists(uint nodeId)
        {
            if (_nodes.All(n => n.Id != nodeId))
            {
                throw new NodeException("Node isn't exists");
            }
        }

        private void ThrowExceptionIfNodeExists(uint nodeId)
        {
            if (_nodes.Any(n => n.Id == nodeId))
            {
                throw new NodeException("Node is already exists");
            }
        }
    }
}
