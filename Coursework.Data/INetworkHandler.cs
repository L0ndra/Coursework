using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data
{
    public interface INetworkHandler : INetwork
    {
        void AddChannel(Channel channel);
        Channel GetChannel(uint firstNodeId, uint secondNodeId);
        void UpdateChannel(Channel newChannel);
        void RemoveChannel(uint firstNodeId, uint secondNodeId);
        void AddNode(Node node);
        void RemoveNode(uint nodeId);
        Channel[] GetChannels(uint nodeId);
    }
}
