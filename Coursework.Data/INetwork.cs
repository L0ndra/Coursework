using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data
{
    public interface INetwork
    {
        Node[] Nodes { get; }
        Channel[] Channels { get; }
        void AddChannel(Channel channel);
        void AddNode(Node node);
        bool IsChannelExists(uint firstNodeId, uint secondNodeId);
        IDictionary<uint, int> GetLinkedNodeIdsWithLinkPrice(uint id);
    }
}