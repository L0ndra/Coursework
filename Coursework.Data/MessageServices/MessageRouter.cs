using System;
using System.Linq;
using Coursework.Data.Constants;
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
            var price = base.CountPrice(startId, destinationId);

            if (double.IsInfinity(price) || Math.Abs(price) < AllConstants.Eps)
            {
                return price;
            }

            var startNode = Network.GetNodeById(startId);

            var channel = Network.GetChannel(startId, destinationId);

            var startMessageQueue = startNode.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            return channel.Price
                    * (channel.ErrorChance + 0.1)
                    * (startMessageQueue.MessagesCount + 1.0);
        }
    }
}
