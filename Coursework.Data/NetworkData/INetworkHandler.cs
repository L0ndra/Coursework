using Coursework.Data.Entities;

namespace Coursework.Data.NetworkData
{
    public interface INetworkHandler : INetwork, IChannelHandler, INodeHandler
    {
        Channel[] GetChannels(uint nodeId);
        void AddInQueue(Message message, uint nodeId);
        void RemoveFromQueue(Message message, uint nodeId);
    }
}
