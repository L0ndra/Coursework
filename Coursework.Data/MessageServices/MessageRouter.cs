using System.Linq;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageRouter : SimpleMessageRouter
    {
        public MessageRouter(INetworkHandler network) : base(network)
        {
        }

        public override double CountPrice(uint startId, uint destinationId)
        {
            if (startId == destinationId)
            {
                return 0.0;
            }

            var startNode = Network.GetNodeById(startId);
            var destinationNode = Network.GetNodeById(destinationId);

            var channel = Network.GetChannel(startId, destinationId);

            if (!startNode.IsActive || !destinationNode.IsActive || channel == null)
            {
                return double.PositiveInfinity;
            }

            var startMessageQueue = startNode.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            return channel.Price * (channel.ErrorChance + 1)
                   * (channel.ErrorChance + 1)
                   * (startMessageQueue.MessagesCount + 1)
                   * (startMessageQueue.MessagesCount + 1);
        }
    }
}
