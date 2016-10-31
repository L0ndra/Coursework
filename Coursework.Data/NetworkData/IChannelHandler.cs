using Coursework.Data.Entities;

namespace Coursework.Data.NetworkData
{
    public interface IChannelHandler
    {
        void AddChannel(Channel channel);
        Channel GetChannel(uint firstNodeId, uint secondNodeId);
        void RemoveChannel(uint firstNodeId, uint secondNodeId);
        void UpdateChannel(Channel newChannel);
    }
}