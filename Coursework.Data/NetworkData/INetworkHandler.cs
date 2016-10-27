using Coursework.Data.Entities;

namespace Coursework.Data.NetworkData
{
    public interface INetworkHandler : INetwork
    {
        void AddChannel(Channel channel);
        Channel GetChannel(uint firstNodeId, uint secondNodeId);
        void UpdateChannel(Channel newChannel);
        void RemoveChannel(uint firstNodeId, uint secondNodeId);
        void AddNode(Node node);
        Node GetNodeById(uint id);
        void RemoveNode(uint nodeId);
        Channel[] GetChannels(uint nodeId);
    }
}
