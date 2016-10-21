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

            AddLink(channel.FirstNodeId, channel.SecondNodeId);

            _channels.Add(channel);
        }

        public IDictionary<uint, int> GetLinkedNodeIdsWithLinkPrice(uint id)
        {
            var channelsFromCurrentNode = _channels.Where(c => c.FirstNodeId == id);
            var channelsToCurrentNode = _channels.Where(c => c.SecondNodeId == id);

            var firstResult = channelsFromCurrentNode
                .ToDictionary(channel => channel.SecondNodeId, channel => channel.Price);

            var secondResult = channelsToCurrentNode
                .ToDictionary(channel => channel.FirstNodeId, channel => channel.Price);

            return firstResult
                .Concat(secondResult)
                .ToDictionary(k => k.Key, k => k.Value);
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

        private Node GetNodeById(uint id)
        {
            return _nodes.FirstOrDefault(n => n.Id == id);
        }

        private void AddLink(uint firstNodeId, uint secondNodeId)
        {
            GetNodeById(firstNodeId).LinkedNodesId.Add(secondNodeId);
            GetNodeById(secondNodeId).LinkedNodesId.Add(firstNodeId);
        }
    }
}
