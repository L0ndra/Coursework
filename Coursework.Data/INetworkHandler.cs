using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data
{
    public interface INetworkHandler : INetwork
    {
        void AddChannel(Channel channel);
        void AddNode(Node node);
        Channel GetChannel(uint firstNodeId, uint secondNodeId);
        void UpdateChannel(Channel newChannel);
        IDictionary<uint, int> GetLinkedNodeIdsWithLinkPrice(uint id);
    }
}
